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

[<DisallowConcurrentExecution>]
type TwitterJob() =
    let context =
        let credentialStore = SingleUserInMemoryCredentialStore()
        credentialStore.ConsumerKey <- ConfigurationManager.AppSettings.Item("consumerKey")
        credentialStore.ConsumerSecret <- ConfigurationManager.AppSettings.Item("consumerSecret")
        credentialStore.AccessToken <- ConfigurationManager.AppSettings.Item("accessToken")
        credentialStore.AccessTokenSecret <- ConfigurationManager.AppSettings.Item("accessTokenSecret")
        let authorizer = new SingleUserAuthorizer()       
        authorizer.CredentialStore <- credentialStore  
        new TwitterContext(authorizer)

    let listen (sinceId: Option<uint64>) = 
        let listenWithSinceId sinceId = 
            printfn "listening to tweets newer than %d" sinceId
            query { 
                for tweet in context.Status do 
                where (tweet.Type = StatusType.Mentions && tweet.SinceID = sinceId)
                select tweet 
            } 

        let listenWithoutSinceId() =
            printfn "listening to all tweet" 
            query { 
                for tweet in context.Status do 
                where (tweet.Type = StatusType.Mentions)
                select tweet 
            } 

        match sinceId with
        | None -> listenWithoutSinceId() 
        | Some(v) -> listenWithSinceId v
    

    interface IJob with
        member this.Execute(context: IJobExecutionContext) = 
            try
                printfn "executing job"
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
                    printfn "no tweet for now..."
            with
            | ex -> printfn "Error: %s" (ex.ToString())
      
    member this.processTweet (tweet: Status) =
        printfn "processing tweet %s from user %s" tweet.Text tweet.User.Name
        match Bot.processRequest tweet.Text with
        | Some(answer) -> this.tweetAnswer answer tweet
        | None -> printfn "nothing to reply"
        printfn "return id=%d" tweet.StatusID
        tweet.StatusID

    member this.tweetAnswer text tweet = 
        try
            let status = sprintf "@%s %s" tweet.User.ScreenNameResponse text
            async { 
                let! reply = context.ReplyAsync(tweet.StatusID, status) |> Async.AwaitTask
                printfn "reply status (%d) %s, %s" reply.StatusID reply.User.Name reply.Text
            } |> Async.RunSynchronously
        with
        | ex -> printfn "%s" ex.Message