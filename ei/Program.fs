open it.mahamudra.imaging
       
                
[<EntryPoint>]
let main argv = 
        let context =  argv.[0]
        let pdf_file_path =  argv.[1]
        let dir_path =  argv.[2]
        let dir_output_images = argv.[3]

        match context with
        | "dir" ->  PDF.scriviImgsFromPdfsInDirectory dir_path dir_output_images
        |_-> PDF.scriviImgsFromPdf pdf_file_path dir_output_images

        printfn "%A" argv
        0 // return an integer exit code