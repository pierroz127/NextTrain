namespace NextTrain.Lib

open System
open System.Configuration
open LinqToTwitter
open Quartz
open Quartz.Job
open NextTrain.Lib

type ITweetIdManager = 
    abstract Save: Id: uint64 -> unit
    abstract Read: unit -> Option<uint64>

type ITweetLogger = 
    abstract logDebug: msg: string -> unit
    abstract logInfo: msg: string -> unit
    abstract logWarn: msg: string -> unit
    abstract logError : msg: string -> unit

    
[<DisallowConcurrentExecution>]
type TwitterJob(config: Configuration, logger: ITweetLogger) =
    let twitterContext =
        let credentialStore = SingleUserInMemoryCredentialStore()
        credentialStore.ConsumerKey <- config.ConsumerKey
        credentialStore.ConsumerSecret <- config.ConsumerSecret
        credentialStore.AccessToken <- config.AccessToken
        credentialStore.AccessTokenSecret <- config.AccessTokenSecret
        let authorizer = new SingleUserAuthorizer()       
        authorizer.CredentialStore <- credentialStore  
        new TwitterContext(authorizer)

    let listen (sinceId: Option<uint64>) = 
        let listenWithSinceId sinceId = 
            sprintf "listening to tweets newer than %d" sinceId |> logger.logInfo
            query { 
                for tweet in twitterContext.Status do 
                where (tweet.Type = StatusType.Mentions && tweet.SinceID = sinceId)
                select tweet 
            } 

        let listenWithoutSinceId() =
            sprintf "listening to all tweet" |> logger.logInfo
            query { 
                for tweet in twitterContext.Status do 
                where (tweet.Type = StatusType.Mentions)
                select tweet 
            } 

        match sinceId with
        | None -> listenWithoutSinceId() 
        | Some(v) -> listenWithSinceId v
    

    interface IJob with
        member this.Execute(context: IJobExecutionContext) = 
            try
                sprintf "executing job" |> logger.logInfo
                let idManager = context.MergedJobDataMap.["lastId"] :?> ITweetIdManager
                let tweets = listen (idManager.Read())
                if Seq.length tweets > 0 
                then
                    tweets
                    |> Seq.toList
                    |> List.map(this.processTweet)
                    |> List.max  
                    |> idManager.Save
                else 
                    sprintf "no tweet for now..." |> logger.logInfo
            with
            | ex -> sprintf "Error: %s %s" (ex.ToString()) ex.StackTrace |> logger.logError
      
    member this.processTweet (tweet: Status) =
        sprintf "processing tweet %s from user %s" tweet.Text tweet.User.Name |> logger.logInfo
        if (tweet.User.ScreenNameResponse <> "proch1departs" || tweet.Text.Contains("#test")) then
            match Bot.processRequest config tweet.Text with
            | Some(answer) -> this.tweetAnswer answer tweet
            | None -> printfn "nothing to reply"
        sprintf "return id=%d" tweet.StatusID |> logger.logInfo
        tweet.StatusID

    member this.tweetAnswer text tweet = 
        try
            let status = sprintf "@%s %s" tweet.User.ScreenNameResponse text
            async { 
                let! reply = twitterContext.ReplyAsync(tweet.StatusID, status) |> Async.AwaitTask
                sprintf "reply status (%d) %s, %s" reply.StatusID reply.User.Name reply.Text |> logger.logInfo
            } |> Async.RunSynchronously
        with
        | ex -> sprintf "%s %s" ex.Message ex.StackTrace |> logger.logError