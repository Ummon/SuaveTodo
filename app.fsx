#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/Npgsql/lib/net45/Npgsql.dll"
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "System.Data.dll"
#r "System.Data.Linq.dll"
#r "System.Data.Entity.dll"
#r "System.Configuration.dll"
#r "FSharp.Data.TypeProviders.dll"

open Suave
open Suave.Web
open Suave.Http 
open Suave.Types
open Suave.Http.Successful
open Suave.Http.Redirection
open Suave.Http.Files
open Suave.Http.RequestErrors
open Suave.Http.Applicatives

open System.Collections.Generic
open System.Linq

open Npgsql
open NpgsqlTypes

open FSharp.Data

type Todos = JsonProvider<"""{"todos" : [{ "id": 42, "text": "aaa" }]}""">

let connection =
    let pw = (System.IO.File.ReadAllText "db-password.txt").Trim()
    let c = new NpgsqlConnection(sprintf "Host=euphorik.ch; Username=todos; password=%s; Database=todos" pw)
    c.Open()
    c
    
let getTodos () : string =
    try
        [|
            use command = new NpgsqlCommand("SELECT id, name FROM todos", connection)
            use reader = command.ExecuteReader()
            while reader.Read() do
                yield Todos.Todo(reader.GetInt32 0, reader.GetString 1)
        |]
    with
        | err -> printfn "Error: %A" err; [| |]
    |> Todos.Root |> string

let add (todo : Choice<string, string>) = 
    match todo with
    | (Choice1Of2 t) -> 
        use command = new NpgsqlCommand("INSERT INTO todos (name) VALUES (:name)", connection)
        command.Parameters.Add("name", NpgsqlDbType.Varchar).Value <- t
        command.ExecuteNonQuery() |> ignore
    | _ -> ()
       
let remove id =
    use command = new NpgsqlCommand("DELETE FROM todos WHERE id = :id", connection)
    command.Parameters.Add("id", NpgsqlDbType.Integer).Value <- id
    command.ExecuteNonQuery() |> ignore

let app : WebPart =
    choose 
        [ GET >>= choose
            [ path "/" >>= file "./static/index.html"
              pathScan "/static/%s" (sprintf "./static/%s" >> file)
              path "/todos" >>= request (fun req -> OK (getTodos ())) ]
          POST >>= choose
            [ path "/todos" >>= request (fun req -> add (req.formData "text") ; OK "" ) ]
          DELETE >>= choose
            [ pathScan "/todos/%d" (fun (id) -> remove id ; OK "") ]
        ]
         