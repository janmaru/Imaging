// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open it.mahamudra.imaging
open System.Drawing
open System
    
[<EntryPoint>]
let main argv = 

    try
//        let ret = new Rectangle(new Point(50,0), new Size(100,100))
//        let extra (i:Image) = 
//          (IMG.extractRectangles i) |> List.map (fun x -> IMG.crop i x)
//
//        PDF.scriviImgsFromPdfsInDirectory 
//               @"C:\DEV\Imaging\test_imaging\"
//               @"D:\temp\"
//               (fun (i: Image) -> i)

        IMG.scriviGeometryImgsFromImage  @"D:\temp\sample.png"  @"D:\temp\" 90 100  120 150


    with 
    | ex -> printfn "%s" (ex.Message.ToString())
 
    printfn "%A" argv
    Console.ReadKey(true)|> ignore
    0 // return an integer exit code
