//   Copyright 2015 Pierre Leroy
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

namespace NextTrain.Lib

open System
open System.Configuration
open LinqToTwitter
open Quartz
open Quartz.Job
open NextTrain.Lib

type IDataManager = 
    abstract SaveId: Id: uint64 -> unit
    abstract ReadId: unit -> Option<uint64>
    abstract SaveCache: tagCache: TagCache -> unit
    abstract ReadCache: unit -> TagCache

type ITweetLogger = 
    abstract logDebug: msg: string -> unit
    abstract logInfo: msg: string -> unit
    abstract logWarn: msg: string -> unit
    abstract logError : msg: string -> unit

type TweetResult = {tweetId: uint64; userName: string; botResult: BotResult;}
    
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
        
    let save (dataManager: IDataManager) (tagCache: TagCache) (tweetResults: TweetResult list) = 
        let maxTweet = tweetResults |> List.maxBy(fun t -> t.tweetId)
        dataManager.SaveId maxTweet.tweetId
        let newTagCache = 
            tweetResults
            |> List.fold(fun (acc: TagCache) t ->
                match t.botResult with 
                | TaggedResult(trip, res) -> acc.add t.userName trip
                | _ -> acc) tagCache
        dataManager.SaveCache newTagCache

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
                let dataManager = context.MergedJobDataMap.["lastId"] :?> IDataManager
                let tagCache = dataManager.ReadCache()
                let tweets = listen (dataManager.ReadId())
                if Seq.length tweets > 0 
                then
                    tweets
                    |> Seq.toList
                    |> List.map(this.processTweet tagCache)
                    |> save dataManager tagCache
                else 
                    sprintf "no tweet for now..." |> logger.logInfo
            with
            | ex -> sprintf "Error: %s %s" (ex.ToString()) ex.StackTrace |> logger.logError
      
    member this.processTweet tagCache (tweet: Status) =
        let userName = tweet.User.ScreenNameResponse 
        sprintf "processing tweet %s from user %s" tweet.Text userName |> logger.logInfo
        if (userName <> "proch1departs" || tweet.Text.Contains("#test")) 
        then
            let res = Bot.processRequest config tagCache userName tweet.Text 
            match res with
            | Result(answer) | TaggedResult(_, answer) -> this.tweetAnswer answer tweet
            | EmptyBotResult -> printfn "nothing to reply"
            sprintf "return id=%d" tweet.StatusID |> logger.logInfo
            {tweetId=tweet.StatusID; userName=userName; botResult=res;}
        else 
            {tweetId=tweet.StatusID; userName=userName; botResult=EmptyBotResult; }

    member this.tweetAnswer text tweet = 
        try
            let status = sprintf "@%s %s" tweet.User.ScreenNameResponse text
            async { 
                let! reply = twitterContext.ReplyAsync(tweet.StatusID, status) |> Async.AwaitTask
                sprintf "reply status (%d) %s, %s" reply.StatusID reply.User.Name reply.Text |> logger.logInfo
            } |> Async.RunSynchronously
        with
        | ex -> sprintf "%s %s" ex.Message ex.StackTrace |> logger.logError