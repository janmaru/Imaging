namespace it.mahamudra.imaging

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

    let exportAsJpegImage(dic_image:PdfDictionary)(count:int byref) =
        use ms = new MemoryStream(dic_image.Stream.Value)
        fileStream fs = new FileStream(String.Format("Image{0}.jpeg", count++), FileMode.Create, FileAccess.Write);
        Image.FromStream(ms) 

    let exportAsPngImage(dic_image:PdfDictionary)(count:int byref) =
        null:>Image
 
    let exportImage(dic_image:PdfDictionary) (count:int byref) =
        let filter = dic_image.Elements.GetName("/Filter") 
        match filter with
        |  "/DCTDecode" -> exportAsJpegImage dic_image &count
        |  "/FlateDecode" ->  exportAsPngImage dic_image &count
        |  "/CCITTFaxDecode" -> exportAsJpegImage dic_image &count
        | _ -> null:>Image

 
    let extractResources (page:PdfPage) =
        let resources:PdfDictionary = page.Elements.GetDictionary("/Resources")  // Get resources dictionary
        resources

    let extractResourcesXObject (resources:PdfDictionary)= 
        let xObject:PdfDictionary = resources.Elements.GetDictionary("/XObject") //:> PdfDictionary   
        xObject

    let extractDictionary (xObjects:PdfDictionary) (imageCount:int byref) =
      let items = xObjects.Elements.Values 
      seq { for (item:PdfItem)  in items do // Iterate references to external objects
              yield item :?>PdfReference }
 

    let extraImageFromReference(reference: PdfReference) (imageCount:int byref)  =
          // we are sure that reference.Value is really a PdfDictionary object, as
          // is the case here.
          let xObject: PdfDictionary  = reference.Value:?>PdfDictionary 
          // Is external object an image?
          if (xObject != null && xObject.Elements.GetString("/Subtype") = "/Image") then
             exportImage xObject &imageCount 
             //The & operator is a way to create a value (a pointer) that can be passed as 
             //an argument to a function/method expecting a byref type. 
          else
             null:>Image
 
    let extractImages (pdf_file_path:string) = 
        let document:PdfDocument  = PdfReader.Open(pdf_file_path)
        let mutable imageCount = 0
        seq { for (page:PdfPage) in document.Pages do // Iterate pages
        // use 'entry.Value' and 'entry.Key' here
                let dic = extractResources page
                let xObject = match dic with
                                | null -> null
                                |_ -> extractResourcesXObject dic
                let xDics = match xObject with
                                | null -> Seq.empty
                                |_ -> extractDictionary xObject &imageCount
                yield xDics |> Seq.map (fun i -> extraImageFromReference i &imageCount)
            }