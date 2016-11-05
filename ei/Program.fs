open System
open it.mahamudra.imaging
       
                
[<EntryPoint>]
let main argv = 
        let dir_or_pdf_path =  argv.[0]
        let dir_output_images = argv.[1]

        try
          PDF.scriviImgsFromPdfsInDirectory dir_or_pdf_path dir_output_images IMG.setJpegResolution2
        with 
        | ex -> printfn "%s" (ex.Message.ToString())

        Console.ReadKey(true)|> ignore
        printfn "%A" argv
        0 // return an integer exit code