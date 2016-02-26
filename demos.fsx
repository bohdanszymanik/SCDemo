open System


// assignment and type inference
let a = 1


// what's the diff between a value and a function... nothing
let addOne = fun x -> x + 1


// make value explicit
let addOne' x = x + 1


// composition
1 |> addOne
1 |> addOne'

let addTwo = fun x -> x + 2

1 |> (addOne >> addTwo)


// custom infix operators
let (>|>) x f = 
    let timer = new System.Diagnostics.Stopwatch()
    timer.Start()
    let r = f x
    printfn "Elapsed Time: %i" timer.ElapsedMilliseconds
    r

[1..1000]
>|> List.map (fun x -> [1..x] |> List.sum)
>|> List.max
>|> fun v -> [1..v]
>|> List.averageBy float


// mutable values - most commonly used in accessing OO classes eg .net framework and performance optimisations
let mutable m = 1
m <- 2


    
// lists, sequences, arrays, computation expressions and higher order functions
let aList = [1..100]
let aSeq = seq{1..1000}
let aSeq' = seq{for x in aList do
                    yield x*x}
let aSeq'' = aList |> List.map (fun x -> x * x) |> Seq.ofList
let anArray = [|0..10..100|]
let anArray' = [|for i in 1..100 -> i * i|]


// pattern matching and indents
let nickNamer name =
    match name with
    | "Robert" -> "Bob"
    | "David" -> "Dave"
    | _ -> name


// nice learner example here https://cockneycoder.wordpress.com/2016/02/16/working-with-running-totals-in-f/
let score = function | "win" -> 2 | "draw" -> 1 | _ -> 0
score "win"
["win"; "draw"; "loss"; "win"; "bye"] |> List.map score

// let's get a running total score for game outcomes
["win"; "draw"; "loss"; "win"; "bye"] |> List.map score |> List.scan (+) 0


// discriminated unions and records - designing with types
// two great intros 
// http://blog.ploeh.dk/2016/02/10/types-properties-software-designing-with-types/
// http://fsharpforfunandprofit.com/series/designing-with-types.html
type Player = PlayerOne | PlayerTwo
type Point = Love | Fifteen | Thirty
type PointsData = { PlayerOnePoint : Point; PlayerTwoPoint : Point }
type FortyData = { Player : Player; OtherPlayerPoint : Point }
type Score =
| Points of PointsData
| Forty of FortyData
| Deuce
| Advantage of Player
| Game of Player


////////////////////////////////////////////
// testing

////////////////////////////////////////////
// 1. refer to Test Project for NUnit, FsUnit, UnQuote examples

////////////////////////////////////////////
// 2. follow property based testing examples below
//      first example - colour operations

#r @"packages\FsCheck\lib\net45\FsCheck.dll"
open FsCheck

type Colour =
        { r : byte
          g : byte
          b : byte
          a : byte }

    let addColour c1 c2 =
        { r = c1.r + c2.r
          g = c1.g + c2.g
          b = c1.b + c2.b
          a = c1.a + c2.a }

let neutral = { r = 0uy; g = 0uy; b = 0uy; a = 0uy }
let someColour = {r=44uy; g=21uy;b=99uy;a=255uy};;

do someColour = (addColour neutral someColour) |> printfn "Should be true: %A"

let ``The operation is commutative`` (a : Colour, b : Colour, c : Colour) =
      addColour a  (addColour b  c) = ( addColour (addColour a b)  c)

Check.Quick ``The operation is commutative``
Check.Verbose ``The operation is commutative``

//      second example - properties on a object over time
type Spacie (fluxCapacitance, darkMatter) =
    let r = new System.Random()
    let speed = r.Next(0,10) * fluxCapacitance * darkMatter
    member this.Speed = speed

let speedCheck f d =
    let s = new Spacie(f,d)
    s.Speed < 100

// let's check that we don't exceed warp 1 - represented by a speed of 100
Check.Quick speedCheck 
Check.Verbose speedCheck
// damn - need to adjust the formula :)


/////////////////////////////////////////////
// 3. testing interactively with Fuchu

#r @"packages\Fuchu\lib\Fuchu.dll"
open Fuchu

let simpleTest = 
    testCase "A simple test" <| 
        fun _ -> Assert.Equal("2+2", 4, 2+2)

run simpleTest;;

/////////////////////////////////////////////
// 4. testing - TickSpec
//      Refer to the test project
//      Nope - I just don't get BDD!


/////////////////////////////////////////////
// csv type provider
#r @"packages\FSharp.Data\lib\net40\FSharp.Data.dll"
open FSharp.Data

type SAPIncidentsProvider = CsvProvider<"incidents.csv">
let incidents = SAPIncidentsProvider.Load(__SOURCE_DIRECTORY__ + "\incidents.csv")

let firstRow = incidents.Rows |> Seq.head
firstRow.``Incident Number``
firstRow.

// etc etc

////////////////////////////////////////////
// deedle
#I "packages/FSharp.Charting"
#I "packages/Deedle"
#load "FSharp.Charting.fsx"
#load "Deedle.fsx"

open System
open Deedle
open FSharp.Charting

let reqs = Frame.ReadCsv(path = __SOURCE_DIRECTORY__ + "\channelRequests.txt", separators = "|")

// it's dummy data so let's enrich it with some random durations...
let r = System.Random()

reqs?Duration <- reqs |> Frame.mapRowValues (fun _ -> r.Next(0,100) )

reqs.Columns.["Service"]
reqs.Columns.["Duration"]

let groupedReqs = reqs.GroupRowsBy<string>("Service")
groupedReqs?Duration |> Stats.levelMean fst

reqs?Duration
|> Series.values

Chart.Histogram(reqs?Duration |> Series.values)

////////////////////////////////////////////
// async

// useful to compare c# and F# here
// http://tomasp.net/blog/csharp-async-gotchas.aspx/
// basically, F# async doesn't return void - it returns Async<T>

let timer1 = new Timers.Timer(500.)
let timer2 = new Timers.Timer(250.)

timer1.Elapsed.Add
    (fun x ->
        printfn "Timer 1: %d:%d" x.SignalTime.Second x.SignalTime.Millisecond)
timer2.Elapsed.Add
    (fun x ->
        printfn "Timer 2: %d:%d" x.SignalTime.Second x.SignalTime.Millisecond)

let runTimer (t : System.Timers.Timer) (s : int) =
    t.Start()
    System.Threading.Thread.Sleep s
    t.Stop()

//Run timers sequentially
runTimer timer1 2040
runTimer timer2 2500

// this doesn't work because runTime just returns unit - needs to be Async<T>
//Async.Parallel [(runTimer timer1 2040); (runTimer timer2 2500)]

// new stuff starts here
let runTimerAsync (t : System.Timers.Timer) s =
    async {
        t.Start()
        // Async workflow needs to do something (basically block here)
        do! Async.Sleep s
        t.Stop()        
    }

// Run timers in parallel
Async.Parallel [(runTimerAsync timer1 5000); (runTimerAsync timer2 5000)]
|> Async.RunSynchronously

// Start with continuations provides us with a way to handle exceptions and cancellation
Async.StartWithContinuations(
    runTimerAsync timer1 2500,
    (fun _ -> printfn "Timer 1 finished"),
    (fun _ -> printfn "Timer 1 threw an exception"),
    (fun _ -> printfn "Cancelled Timer 1"))

Async.StartWithContinuations(
    runTimerAsync timer2 1500,
    (fun _ -> printfn "Timer 2 finished"),
    (fun _ -> printfn "Timer 2 threw an exception"),
    (fun _ -> printfn "Cancelled Timer 2"))


// mailboxprocessor
let agent = MailboxProcessor.Start(fun inbox ->
    let rec receiveMessages = async{
        let! msg = inbox.Receive()
        printfn "received: %s" msg
        return! receiveMessages
    }
    receiveMessages
    )

agent.Post("Hello")

// mailboxprocessors work on lightweight threads - run up a million - no prob
let agents =
    [|0 .. 1000000|]
    |> Array.map (fun i -> 
                    MailboxProcessor.Start(fun inbox ->
                        async {
                            while true do
                                let! msg = inbox.Receive()
                                if i % 10000 = 0 then printfn "Agent %d got msg %s" i msg
                        }))

for agent in agents do
    agent.Post "Ping"

/////////////////////////////////////////////
// web programming - Suave.IO and WebSharper


(* suave.io - non-blocking web server

Idea is to chain webparts where webparts take some http context and result in async of httpcontext option

type WebPart = HttpContext -> Async<HttpContext option>

default port 8083

*)
#r @"packages\Suave\lib\net40\Suave.dll"

open Suave

startWebServer defaultConfig (Successful.OK "Hello World!")


open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

let app =
  choose
    [ GET >=> choose
        [ path "/hello" >=> OK "Hello GET"
          path "/goodbye" >=> OK "Good bye GET" ]
      POST >=> choose
        [ path "/hello" >=> OK "Hello POST"
          path "/goodbye" >=> OK "Good bye POST" ] ]

startWebServer defaultConfig app

(*
This is a good example of railway oriented programming http://fsharpforfunandprofit.com/posts/recipe-part2/

The >=> is used to chain Async options of type httpcontext
*)

// websharper - sitelets, formlets, piglets best viewed in website http://websharper.com/
