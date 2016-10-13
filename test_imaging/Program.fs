// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open it.mahamudra.imaging
open System.Drawing
    
[<EntryPoint>]
let main argv = 
    let imgs = OCR.extractListImages @"C:\DEV\Imaging\test_imaging\1.pdf"
    let mutable count:int = 0
    for f in imgs do
         let btmps = f |> Seq.map (fun (z:Image) -> if isNull(z) then null else new Bitmap(z))|> seq<Bitmap>
         for b in btmps do
          count <- count+1
          if isNull(b) then printfn "" else b.Save((sprintf @"C:\DEV\Imaging\test_imaging\%d.jpg" count)) |>ignore 

    printfn "%A" argv
    0 // return an integer exit code
