using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace AzArtFunc
{
    class Program
    {
        static void Main(string[] args)
        {
            AzArtifactService office = new AzArtifactService("office-conn-str");
            var officeData = office.GiveMeData("001");

            //AzArtifactService bing = new AzArtifactService("bing-conn-str");
            //var bingData = office.GiveMeData("002");
            GitHubArtifactService bing = new GitHubArtifactService("bing-github-conn-str", "github-bing-sa");
            var bingData = office.GiveMeData("002");

            GitHubArtifactService edge = new GitHubArtifactService("edge-conn-str", "github-edge-sa");
            var edgeedgeData = office.GiveMeData("002");


            var interopServices = new Dictionary<IteropPackageType, InteropPkgSource>();
            interopServices.Add(IteropPackageType.Office, office);
            interopServices.Add(IteropPackageType.Bing, bing);
            interopServices.Add(IteropPackageType.Edge, edge);

            //interopServices.Add(IteropPackageType.otherservices, new AzArtifactService("ohter-conn-str"));
            // sometime later
            InteropPackage serviceLater = new InteropPackage(interopServices);

            var officePkg001 = serviceLater.GetPackage(IteropPackageType.Office, "001");
            var bingPkg002 = serviceLater.GetPackage(IteropPackageType.Bing, "002");
            var edgePkg003 = serviceLater.GetPackage(IteropPackageType.Edge, "003");

            var other = serviceLater.GetPackage(IteropPackageType.Office, "001");
        }
    }

    public class OSUpdateEndpointFrontEnd
    {
        InteropPackage interopPackage;

        public OSUpdateEndpointFrontEnd(InteropPackage backendService)
        {
            this.interopPackage = backendService;
        }

        public IteropPackageBlob GetPackage(IteropPackageType pkgType, string id)
        {
            return this.interopPackage.GetPackage(pkgType, id);
        }
    }

    public enum IteropPackageType { Unknown, Office, Bing, Edge };
    public enum BlobSource { Unknown, GitHub, AzArtifact, GitLab, StorageAccount, OtherService };

    public class InteropPackage
    {
        Dictionary<IteropPackageType, InteropPkgSource> interopServices = new Dictionary<IteropPackageType, InteropPkgSource>();

        public InteropPackage(
            AzArtifactService office,
            AzArtifactService bing)
        {
            interopServices.Add(IteropPackageType.Office, office);
            interopServices.Add(IteropPackageType.Bing, bing);
        }

        public InteropPackage(Dictionary<IteropPackageType, InteropPkgSource> interopsServices)
        {
            this.interopServices = interopsServices;
        }

        public StorageAccountBlob GetPackage(IteropPackageType pkgType, string id)
        {
            if (this.interopServices.ContainsKey(pkgType))
            {
                return this.interopServices[pkgType].GetPackage(id);
            }

            throw new Exception("not supported"); // return not found
        }
    }

    public interface InteropPkgSource
    {
        GetPackageOperation<StorageAccountBlob> GetPackage(string id);
    }

    public class GitHubArtifactService : InteropPkgSource
    {
        private string connStr;
        private string storageAccount;
        OSUpdateStorageAccount osUpdSa;

        public GitHubArtifactService(string connStr, string storageAccount)
        {
            this.connStr = connStr;
            this.storageAccount = storageAccount;
        }

        public GetPackageOperation<StorageAccountBlob> GetPackage(string id)
        {
            var blob = osUpdSa.GetBlob(id);

            if (blob == null)
            {
                var githubBlob = GiveMeData(id);
                if (githubBlob != null)
                {
                    osUpdSa.SaveData(id, ToByteArray(githubBlob));
                    blob = osUpdSa.GetBlob(id);
                }
            }

            return blob;

            //var githubBlob = GiveMeData(id);
            //return new IteropPackageBlob() { SourceType = BlobSource.AzArtifact, urlWithSasUrl = githubBlob.GitHubSpecificUrl };
        }

        public GitHubBlob GiveMeData(string id)
        {
            return new GitHubBlob() { GitHubSpecificUrl = $"GITHUB:give-data-from-{this.connStr}-{this.storageAccount}-with-id-{id}" };
        }

        private byte[] ToByteArray(GitHubBlob blob)
        {
            return new byte[0];
        }
    }

    public class GitHubBlob
    {
        public string GitHubSpecificUrl;
    }

    public class AzArtifactService : InteropPkgSource
    {
        private string connStr;
        OSUpdateStorageAccount osUpdSa;
        public AzArtifactService(string connStr)
        {
            this.connStr = connStr;
        }

        public GetPackageOperation<StorageAccountBlob> GetPackageOperation(string id)
        {
            //var azArtifact = GiveMeData(id);
            //return new IteropPackageBlob() { SourceType = BlobSource.AzArtifact, urlWithSasUrl = azArtifact.AzArtficatSpecificData };

            var blob = osUpdSa.GetBlob(id);
            if (blob == null)
            {
                StorageAccountCopyOperation azArtifactCopyOperation = GiveMeData(id);

                //if(azArtifactCopyOperation.Status != "Success")
                //{
                //    // what do we do?
                //    if(azArtifactCopyOperation.Status != "Failed")
                //    {
                //        // what do we do?

                //        // throw an exception? or return a failed operation.
                //        // retry? ---> how many? and if it fails all retries? what do we do? throw an exception?

                //        return new GetPackageOperation<StorageAccountBlob>()
                //        {
                //            Status = "AzArtifactFailedToCopy",
                //            ErrorMessage = azArtifactCopyOperation.ErrorMessage,
                //            Data = null,
                //        };
                //    }
                //}

                //if(azArtifactCopyOperation.Status == "Success")
                //{
                //    var success = TryCopyToOSUpdateStorageAccount(azArtifactCopyOperation.Data);
                //    if (success)
                //    {
                //        blob = osUpdSa.GetBlob(id);
                //    }
                //}

                if (azArtifactCopyOperation != null && azArtifactCopyOperation.Status != "Success")
                {
                    //?
                    // throw an exception? or return a failed operation.
                    // retry? ---> how many? and if it fails all retries? what do we do? throw an exception?

                    return new GetPackageOperation<StorageAccountBlob>()
                    {
                        Status = "FailedToCopy",
                        ErrorMessage = "FailedToCopy After X Attempts",
                        Data = null,
                    };
                }
            }

            return new GetPackageOperation<StorageAccountBlob> { Status = "Success", Data = blob };
        }

        public StorageAccountCopyOperation GiveMeData(string id)
        {
            // coping from AzStorageAccount to OSUpdateAccount
            return new StorageAccountCopyOperation()
            {
                Status = "Success",
                Data = new AzArtifactBlob() { AzArtficatSpecificData = $"Azure:give-data-from-{this.connStr}-with-id-{id}" }
            };
        }

        private byte[] ToByteArray(AzArtifactBlob blob)
        {
            return new byte[0];
        }

        private bool TryCopyToOSUpdateStorageAccount(AzArtifactBlob blob)
        {

            return false;
        }
    }

    public class StorageAccountCopyOperation
    {
        public string Status;
        public string ErrorMessage;
        public AzArtifactBlob Data;
    }

    public class GetPackageOperation<T>
    {
        public string Status;
        public string ErrorMessage;
        public T Data;
    }

    public class AzArtifactBlob
    {
        public string AzArtficatSpecificData;
    }

    public class StorageAccountBlob
    {
        public string blobUrl;
    }

    public class OSUpdateStorageAccount
    {
        public void SaveData(string id, byte[] data)
        {
            // save data to storage account
        }

        public StorageAccountBlob GetBlob(string id)
        {
            // if it does not exist
            // return null; // not found

            // get blob sas uri
            throw new NotImplementedException();
        }
    }

    public class IteropPackageBlob
    {
        public BlobSource SourceType;
        public string urlWithSasUrl;
    }
}
