using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace CTecUtil.Printing
{
    public class Packages
    {
        /// <summary>Creates a package zip file containing specified content and resource files.</summary>
        private static void CreatePackage(string packagePath, string documentPath, string relationshipType)
        {
            // Convert system path and file names to Part URIs. 
            Uri partUriDocument = PackUriHelper.CreatePartUri(new Uri(documentPath, UriKind.Relative));
            //Uri partUriResource = PackUriHelper.CreatePartUri(new Uri(resourcePath, UriKind.Relative));

            // Create the Package
            using (Package package = Package.Open(packagePath, FileMode.Create))
            {
                // Add the Document part to the Package
                PackagePart packagePartDocument = package.CreatePart(partUriDocument, System.Net.Mime.MediaTypeNames.Text.Xml);

                // Copy the data to the Document Part
                using (FileStream fileStream = new FileStream(documentPath, FileMode.Open, FileAccess.Read))
                {
                    CopyStream(fileStream, packagePartDocument.GetStream());
                }

                // Add a Package Relationship to the Document Part
                package.CreateRelationship(packagePartDocument.Uri, TargetMode.Internal, relationshipType);

                //// Add a Resource Part to the Package
                //PackagePart packagePartResource = package.CreatePart(partUriResource, System.Net.Mime.MediaTypeNames.Image.Jpeg);
                //
                //// Copy the data to the Resource Part
                //using (FileStream fileStream = new FileStream(resourcePath, FileMode.Open, FileAccess.Read))
                //{
                //    CopyStream(fileStream, packagePartResource.GetStream());
                //}
                //
                //// Add Relationship from the Document part to the Resource part
                //packagePartDocument.CreateRelationship(
                //                        new Uri(@"../resources/image1.jpg",
                //                        UriKind.Relative),
                //                        TargetMode.Internal,
                //                        ResourceRelationshipType);
            }
        }

        
        private static void CopyStream(Stream source, Stream target)
        {
            const int bufSize = 0x1000;
            byte[] buf = new byte[bufSize];
            int bytesRead = 0;
            while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
                target.Write(buf, 0, bytesRead);
        }
    }
}
