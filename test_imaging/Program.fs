// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open it.mahamudra.imaging
open System.Drawing
open System
    
[<EntryPoint>]
let main argv = 
    try
        PDF.scriviImgsFromPdfsInDirectory @"C:\DEV\Imaging\test_imaging\" @"D:\temp\" 
    with 
    | ex -> printfn "%s" (ex.Message.ToString())
 
    printfn "%A" argv
    Console.Read |> ignore
    0 // return an integer exit code
