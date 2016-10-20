// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open it.mahamudra.imaging
open System.Drawing
    
[<EntryPoint>]
let main argv = 
        let imgs = PDF.extractImages @"C:\DEV\Imaging\test_imaging\1.pdf"
        let mutable count:int = 0
        for i:Image in imgs do
            count<-count+1
            i.Save((sprintf @"C:\DEV\Imaging\test_imaging\%d.jpg" count)) |>ignore 
        printfn "%A" argv
        0 // return an integer exit code
