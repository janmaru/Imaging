namespace it.mahamudra.imaging
open System

type Dangerous(comment:string) = inherit System.Attribute();

module OCR =
    open PdfSharp.Pdf
    open PdfSharp.Pdf.IO
    open PdfSharp.Pdf.Advanced
    open System.Collections
    open System.Drawing
    open System.IO

    ///ByRef is a special type that can be (reasonably) used only in method parameters. 
    /// It means that the argument should be essentially a pointer to some memory location (allocated on heap or stack).
    /// It corresponds to out and ref modifiers in C#. Note that you cannot create local variable of this type.

    let exportAsJpegImage(dic_image:PdfDictionary)  =
        try
            use ms = new MemoryStream(dic_image.Stream.Value)
            Image.FromStream(ms) 
        with
           |_ -> null // :>Image

    let exportAsPngImage(dic_image:PdfDictionary)  =
        null:>Image
 
    let exportImage(dic_image:PdfDictionary)   =
        let filter = dic_image.Elements.GetName("/Filter") 
        match filter with
        |  "/DCTDecode" -> exportAsJpegImage dic_image  
        |  "/FlateDecode" ->  exportAsPngImage dic_image 
        |  "/CCITTFaxDecode" -> exportAsJpegImage dic_image  
        | _ -> null:>Image

 
    let extractResources (page:PdfPage) =
        let resources:PdfDictionary = page.Elements.GetDictionary("/Resources")  // Get resources dictionary
        resources

    let extractResourcesXObject (resources:PdfDictionary)= 
        let xObject:PdfDictionary = resources.Elements.GetDictionary("/XObject") //:> PdfDictionary   
        xObject

    [<Dangerous("Do not use. Use List instead.")>]
    let extractSeqDictionary (xObjects:PdfDictionary) =
      let items = xObjects.Elements.Values 
      seq { for (item:PdfItem)  in items do // Iterate references to external objects
              yield item :?>PdfReference }
 
    let extractListDictionary (xObjects:PdfDictionary) =
      let items = xObjects.Elements.Values 
      [ for (item:PdfItem)  in items do // Iterate references to external objects
              yield item :?>PdfReference ]

    let extraImageFromReference(reference: PdfReference)  =
          // we are sure that reference.Value is really a PdfDictionary object, as
          // is the case here.
          let xObject: PdfDictionary  = reference.Value:?>PdfDictionary 
          // Is external object an image?
          if (xObject <> null && xObject.Elements.GetString("/Subtype") = "/Image") then
             exportImage xObject  
             //The & operator is a way to create a value (a pointer) that can be passed as 
             //an argument to a function/method expecting a byref type. 
          else
             null:>Image
 
    [<Dangerous("Do not use. Use List instead.")>]
    let extractSeqImages (pdf_file_path:string) = 
        let document:PdfDocument  = PdfReader.Open(pdf_file_path)
        seq { for (page:PdfPage) in document.Pages do // Iterate pages
        // use 'entry.Value' and 'entry.Key' here
                let dic = extractResources page
                let xObject = match dic with
                                | null -> null
                                |_ -> extractResourcesXObject dic
                let xDics = match xObject with
                                | null -> Seq.empty
                                |_ -> extractSeqDictionary xObject
                yield xDics |> Seq.map (fun i -> extraImageFromReference i)
            }

    let extractListImages (pdf_file_path:string) = 
        let document:PdfDocument  = PdfReader.Open(pdf_file_path)
        [for (page:PdfPage) in document.Pages do // Iterate pages
        // use 'entry.Value' and 'entry.Key' here
                let dic = extractResources page
                let xObject = match dic with
                                | null -> null
                                |_ -> extractResourcesXObject dic
                let xDics = match xObject with
                                | null -> List.empty
                                |_ -> extractListDictionary xObject
                yield xDics |> Seq.map (fun i -> extraImageFromReference i)
         ]