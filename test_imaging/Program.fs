// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open it.mahamudra.imaging
open System.Drawing
    
[<EntryPoint>]
let main argv = 
    let printImg f:seq<Image> =
        let btmps = f |> Seq.map (fun (z:Image) -> new Bitmap(z))|> seq<Bitmap>
        btmps |> Seq.iter(fun p-> p.Save(""))

    let imgs = OCR.extractImages "C:\DEV\Imaging\test_imaging\1.pdf"
    imgs
       |> Seq.iter (fun i -> printImg i)
    printfn "%A" argv
    0 // return an integer exit code
