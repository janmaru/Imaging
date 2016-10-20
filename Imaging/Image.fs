namespace it.mahamudra.imaging
open System

type Dangerous(comment:string) = inherit System.Attribute();

module PDF = 
     open Spire.Pdf
     open System.Drawing
     let extractImages (pdf_file_path:string) =
         let doc:PdfDocument  = new PdfDocument() 
         doc.LoadFromFile(pdf_file_path)
         let pages = doc.Pages
         seq { for page:PdfPageBase in pages do
                 let imgs =  page.ExtractImages()
                 for img:Image in imgs do
                    yield img}
 