using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Encodings.Web;

namespace WebRequestExample;

public class RequestManager {

    private string CMSURL = "http://localhost:8055";
    private string WebAppBackendURL = "http://localhost:8080";

    private LoginData parsedLoginData;
    private UserData loggedInUserData;
    private ProgressData userProgressData;
    private CustomerModuleData customerModuleVRData;

    public RequestManager(){
        /****** TEMPORARY: Only for self signed certs ********/
        ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(this.AcceptAllCertifications);
        /*****************************************************/

        // If dotnet doesn't have TLS1.2 enabled for cert verification by default, then you might have to uncomment the line below
        // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        this.customerModuleVRData = new CustomerModuleData();
    }
    /****** TEMPORARY: Only for self signed certs ********/
    private bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors){return true;}
    /*****************************************************/


    public void MakeRequest(int requestType){
        switch(requestType){
            case 1:
                this.LoginUser();
                break;
            case 2:
                this.ValidateToken(this.parsedLoginData?.data?.access_token);
                break;
            case 3:
                this.LogoutUser();
                break;
            case 4:
                this.SendResetLink();
                break;
            case 5:
                this.ResetPassword();
                break;
            case 6:
                this.GetMyUserData();
                break;
            case 7:
                this.GetMyProgressData();
                break;
            case 8:
                this.CreateMyProgressData();
                break;
            case 9:
                this.UpdateMyProgressData();
                break;
        }
    }

    private void LoginUser(){
        bool tokenValid = false;
        if(this.parsedLoginData is null){
            var fileInfo = new FileInfo(".\\logincookie.json");
            if(!(fileInfo.Length == 0)){
                string cookieContent = File.ReadAllText(".\\logincookie.json");
                LoginData tempLoginData = JsonSerializer.Deserialize<LoginData>(cookieContent);
                if(!string.IsNullOrEmpty(tempLoginData?.data?.access_token)){
                    Console.WriteLine("\nAuth Token Exists in Cookie. Validating Token...\n");
                    tokenValid = this.ValidateToken(tempLoginData?.data?.access_token);
                    if(tokenValid){
                        Console.WriteLine("\nToken Valid. Auto Logging In And Getting User Data...\n");
                        this.parsedLoginData = tempLoginData;
                        this.GetMyUserData();
                    }
                    else{
                        Console.WriteLine("\nToken Invalid. Redirecting To Login...\n");
                        var fs = File.Create(".\\logincookie.json");
                        fs.Close();
                    }
                }
            }
        }
        
        if(!(this.parsedLoginData is null) && !tokenValid){
            if(!string.IsNullOrEmpty(this.loggedInUserData?.data?.email)) Console.WriteLine("\nAlready logged in as " + this.loggedInUserData.data.email + ". Please logout first.\n");
            else Console.WriteLine("\nAlready logged in. Please logout first.\n");
        }
        else if(!tokenValid){
            const string REGEXPATTERN = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,})+)$";
            Console.WriteLine("\nPlease type login email address, then press enter.\n");
            string login_email = Console.ReadLine();
            Match m = Regex.Match(login_email, REGEXPATTERN);
            if(!m.Success){
                Console.WriteLine("\nError: Email address must have valid email syntax.\n");
            }
            else{
                Console.WriteLine("\nPlease type login password, then press enter.\n");
                string login_password = Console.ReadLine();
                Console.WriteLine("\n");
                string requestPayload = @"
                    {
                        ""email"": """ + login_email + @""",
                        ""password"": """ + login_password + @"""
                    }
                ";
                // string requestPayload = @"
                //     {
                //         ""email"": ""my@email.com"",
                //         ""password"": ""password""
                //     }
                // ";
                string loginResponseString = this.MakeComplexRequest("POST", CMSURL + "/auth/login", requestPayload);
                if(!loginResponseString.Contains("errors")){
                    FireWriter.Write("logincookie.json", loginResponseString);

                    this.parsedLoginData = JsonSerializer.Deserialize<LoginData>(loginResponseString);

                    Console.WriteLine($"\nAccess Token [httresponse.data.access_token]: {this.parsedLoginData.data.access_token}");
                    Console.WriteLine($"Expiration TTL [httresponse.data.expires]: {this.parsedLoginData.data.expires}");
                    Console.WriteLine($"Refresh Token [httresponse.data.refresh_token]: {this.parsedLoginData.data.refresh_token}\n");

                    Console.WriteLine("\nGetting User Data...\n");
                    this.GetMyUserData();
                }
            }
        }
    }

    private bool ValidateToken(string? access_token){
        if(string.IsNullOrEmpty(access_token) || access_token==null){
            Console.WriteLine("\nNo login data available. Please login first.\n");
            return false;
        }
        else{
            string userDataResponseString = this.MakeGetRequest(CMSURL + "/items/dummy", null, access_token);
            // Dummy Item ID will always be null. This is because the table contains no data. This table is just to test access_token validation.
            bool logginFailed = string.IsNullOrEmpty(userDataResponseString) || userDataResponseString.Contains("errors");
            bool loginValid = userDataResponseString.Contains("data");
            if(!logginFailed && loginValid) return true;
            else return false;
        }
    }

    private void LogoutUser(){
        if(this.parsedLoginData is null){
            Console.WriteLine("\nNo login data available. Please login first.\n");
        }
        else{
            string requestPayload = @"
                {
                    ""refresh_token"": """ + this.parsedLoginData?.data?.refresh_token + @"""
                }
            ";
            string logoutResponseString = this.MakeComplexRequest("POST", CMSURL + "/auth/logout", requestPayload);
            this.parsedLoginData = null;
            if(!(this.loggedInUserData is null)) this.loggedInUserData = null;
            var fs = File.Create(".\\logincookie.json");
            fs.Close();
            Console.WriteLine("\nSuccessfully logged out.\n");
        }
    }

    private void SendResetLink(){
        if(!(this.parsedLoginData is null)){
            Console.WriteLine("\nPlease logout first.\n");
        }
        else{
            Console.WriteLine("\nPlease type email address to send password reset email to and then hit enter.\n");
            string email = Console.ReadLine();
            Console.WriteLine("\n");
            string requestPayload = @"
                {
                    ""email"": """ + email + @"""
                }
            ";
            this.MakeComplexRequest("POST", WebAppBackendURL + "/reset", requestPayload);
            Console.WriteLine("\nIf a reset email is successfully sent, response will contain \"OK\" text with response status code as 200.\nIf unsuccessful, response will contain stringifed JSON with details of the error.\n");
        }
    }

    private void ResetPassword(){
        if(!(this.parsedLoginData is null)){
            Console.WriteLine("\nPlease logout first.\n");
        }
        else{
            Console.WriteLine("\nPlease type reset token then press enter.\n");
            string reset_token = Console.ReadLine();
            Console.WriteLine("\nPlease type new password then press enter.\n");
            string new_password = Console.ReadLine();
            Console.WriteLine("\n");
            string requestPayload = @"
                {
                    ""token"": """ + reset_token + @""",
                    ""password"": """ + new_password + @"""
                }
            ";
            this.MakeComplexRequest("POST", CMSURL + "/auth/password/reset", requestPayload);
            Console.WriteLine("\nIf password was successfully reset, response will contain \"OK\" text with response status code as 200.\nIf unsuccessful, response will contain stringifed JSON with details of the error.\n");
        }
    }

    private void GetMyUserData(){
        if(this.parsedLoginData is null){
            Console.WriteLine("\nNo login data available. Please login first.\n");
        }
        else{
            string requestQueries = "fields=id,first_name,last_name,email";
            string userDataResponseString = this.MakeGetRequest(CMSURL + "/users/me", requestQueries, this.parsedLoginData.data.access_token);
            this.loggedInUserData = JsonSerializer.Deserialize<UserData>(userDataResponseString);

            Console.WriteLine($"\nUser ID [httresponse.data.id]: {this.loggedInUserData.data.id}");
            Console.WriteLine($"Firstname [httresponse.data.first_name]: {this.loggedInUserData.data.first_name}");
            Console.WriteLine($"Lastname [httresponse.data.last_name]: {this.loggedInUserData.data.last_name}");
            Console.WriteLine($"Email [httresponse.data.email]: {this.loggedInUserData.data.email}\n");
        }
    }

    private void GetMyProgressData(){
        if(this.loggedInUserData is null){
            Console.WriteLine("\nNo user data available. Please login and get user data.\n");
        }
        else{
            string requestQueries = "filter[user_id][_eq]="+this.loggedInUserData.data.id;
            string progressResponseString = this.MakeGetRequest(CMSURL + "/items/progress", requestQueries, this.parsedLoginData.data.access_token);
            try{
                this.userProgressData = JsonSerializer.Deserialize<ProgressData>(progressResponseString);
                this.userProgressData.processedProgressData = JsonSerializer.Deserialize<ProcessedProgressData>(this.userProgressData.data[0].progress_data);

                // This shows partial relationship between data being stored in API and VR, and how to update either. You can also reference UpdateMyProgressData for more details.
                this.UpdateVRORProgressData(true);
            }
            catch(Exception e){
                this.userProgressData = null;
                if(progressResponseString.Trim().Equals("{\"data\":[]}")){
                    Console.WriteLine("\nNo progress data available for this user. Creating new progress data entry for this user in DB.\n");
                    this.CreateMyProgressData();
                }
            }
        }
    }

    private void CreateMyProgressData(){
        if(this.userProgressData?.data?.Length > 0){        // I don't think this is a good conditional check, but for now it works
            Console.WriteLine("\nProgress data already exists. You should not create more than one progress data per user.\n");
        }
        else if(this.loggedInUserData is null){
            Console.WriteLine("\nNo user data available. You must first login and get user data.\n");
        }
        else{
            string progressDataFileContent = File.ReadAllText(".\\ProgressDataTemplate.json");

            ProcessedProgressData processedProgressData = JsonSerializer.Deserialize<ProcessedProgressData>(progressDataFileContent);

            for(int i=0; i<processedProgressData.easterEgg.Length; i++){
                processedProgressData.easterEgg[i].id = Guid.NewGuid().ToString();
            }
            for(int i=0; i<processedProgressData.learningModule.Length; i++){
                processedProgressData.learningModule[i].id = Guid.NewGuid().ToString();
                for(int j=0; j<processedProgressData.learningModule[i].touchPoints.Length; j++){
                    processedProgressData.learningModule[i].touchPoints[j].id = Guid.NewGuid().ToString();
                }
            }
            JsonSerializerOptions serializationOptions = new JsonSerializerOptions();
			serializationOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;	

            string stringifiedProgressDataTemplate = JsonSerializer.Serialize(processedProgressData);
            //stringifiedProgressDataTemplate = JsonSerializer.Serialize(stringifiedProgressDataTemplate, serializationOptions);

            ProgressData newProgressData = new ProgressData();
            newProgressData.data = new ProgressData.InternalData[]{new ProgressData.InternalData()};
            newProgressData.data[0].id = Guid.NewGuid().ToString();
            newProgressData.data[0].updated_on = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            newProgressData.data[0].user_id = this.loggedInUserData.data?.id;
            newProgressData.data[0].progress_data = stringifiedProgressDataTemplate;
            newProgressData.processedProgressData = processedProgressData;

            string newProgressDataPayload = JsonSerializer.Serialize(newProgressData.data[0], serializationOptions);

            // Response String will look very similar to payload object
            string logoutResponseString = this.MakeComplexRequest("POST", CMSURL + "/items/progress", newProgressDataPayload, this.parsedLoginData.data.access_token, true);

            this.userProgressData = newProgressData;

            // This shows partial relationship between data being stored in API and VR, and how to update either. You can also reference UpdateMyProgressData for more details.
            this.UpdateVRORProgressData(true);
        }
    }

    private void UpdateMyProgressData(){
        if(this.userProgressData?.data?.Length == 0){        // I don't think this is a good conditional check, but for now it works
            Console.WriteLine("\nNo progress data available. Please get progress data before continuing.\n");
        }
        else{
            this.customerModuleVRData.ModifyCustomModuleData();

            this.UpdateVRORProgressData(false);

            string stringifiedProgressData = JsonSerializer.Serialize(this.userProgressData.processedProgressData);

            this.userProgressData.data[0].progress_data = stringifiedProgressData;
            this.userProgressData.data[0].updated_on = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            JsonSerializerOptions serializationOptions = new JsonSerializerOptions();
			serializationOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;	

            string updatedProgressDataPayload = JsonSerializer.Serialize(this.userProgressData.data[0], serializationOptions);

            string logoutResponseString = this.MakeComplexRequest("PATCH", CMSURL + "/items/progress/" + this.userProgressData.data[0].id, updatedProgressDataPayload, this.parsedLoginData.data.access_token, true);
        }
    }


    private void UpdateVRORProgressData(bool updateVRData) {
        if(updateVRData){
            for(int i=0; i<this.userProgressData.processedProgressData.learningModule.Length; i++){
                // Checking against Meet Your Customers module
                if(i==0){
                    try{
                    int completedCheckPoints = 0;
                    for(int j=0; j<this.customerModuleVRData.checkpointIDDataDictionary._values.Length; j++){
                        if(this.userProgressData.processedProgressData.learningModule[i].touchPoints[j].completed){
                            completedCheckPoints += 1;
                        }
                        this.customerModuleVRData.checkpointIDDataDictionary._values[j] = this.userProgressData.processedProgressData.learningModule[i].touchPoints[j].completed;
                    }
                    if(completedCheckPoints==this.customerModuleVRData.checkpointIDDataDictionary._values.Length){
                        this.customerModuleVRData.progressState = (int)EnumSets.CompletionStatus.Completed;
                    }
                    }
                    catch(Exception e){Console.WriteLine(e);}
                }
            }
            //Console.WriteLine("Entered Here: " + this.userProgressData.processedProgressData.learningModule[0].touchPoints.Length);
        }
        else{
            for(int i=0; i<this.userProgressData.processedProgressData.learningModule.Length; i++){
                // Checking against Meet Your Customers module
                if(i==0){
                    int completedCheckPoints = 0;
                    for(int j=0; j<this.customerModuleVRData.checkpointIDDataDictionary._keys.Length; j++){
                        if(this.customerModuleVRData.checkpointIDDataDictionary._values[j]){
                            completedCheckPoints += 1;
                        }
                        this.userProgressData.processedProgressData.learningModule[i].touchPoints[j].completed = this.customerModuleVRData.checkpointIDDataDictionary._values[j];
                    }
                    if(completedCheckPoints==this.customerModuleVRData.checkpointIDDataDictionary._keys.Length){
                        this.userProgressData.processedProgressData.learningModule[i].completed = (int)EnumSets.CompletionStatus.Completed;
                    }
                }
            }
        }
    }



    private string MakeComplexRequest(string method, string requestURL, string requestPayload, string bearerToken = null, bool showResponsePayload = true){
        try{
            var complexHttpWebRequest = (HttpWebRequest)WebRequest.Create(requestURL);
            complexHttpWebRequest.ContentType = "application/json";
            complexHttpWebRequest.Accept = "application/json";
            complexHttpWebRequest.Method = method;
            if(!string.IsNullOrEmpty(bearerToken)){
                complexHttpWebRequest.Headers.Add("Authorization", "Bearer " + bearerToken);
            }

            using (var streamWriter = new StreamWriter(complexHttpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(requestPayload);
            }

            var httpResponse = (HttpWebResponse)complexHttpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var httpResponseString = streamReader.ReadToEnd();
                Console.WriteLine("Successful Http Response\n");
                if(showResponsePayload) Console.WriteLine("Status Code: " + httpResponse.StatusCode + "\nStringified Response Object: " + httpResponseString + "\n");
                return httpResponseString;
            }
        }
        catch(WebException webExp){
            if(webExp.Status == WebExceptionStatus.ProtocolError && webExp.Response != null){
                var httpResponse = (HttpWebResponse) webExp.Response;
                var httpResponseString = new StreamReader(httpResponse.GetResponseStream()).ReadToEnd();

                Console.WriteLine("Unsuccessful Http Response\n");
                if(showResponsePayload) Console.WriteLine("Status Code: " + httpResponse.StatusCode + "\nStringified Response Object: " + httpResponseString + "\n");
                return httpResponseString;
            }
            else{
                Console.WriteLine("Could not send a successful request. Something went wrong.\n");
                return "";
            }
        }
    }

    private string MakeGetRequest(string requestURL, string requestQueries = null, string bearerToken = null, bool showResponsePayload = true){
        string processedRequestURL = requestURL + (string.IsNullOrEmpty(requestQueries) ? "" : "?"+requestQueries);

        try{
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(processedRequestURL);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            if(!string.IsNullOrEmpty(bearerToken)){
                httpWebRequest.Headers.Add("Authorization", "Bearer " + bearerToken);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            var httpResponseString = new StreamReader(httpResponse.GetResponseStream()).ReadToEnd();

            Console.WriteLine("Successful Http Response:");
            if(showResponsePayload) Console.WriteLine("Status Code: " + httpResponse.StatusCode + "\nStringified Response Object: " + httpResponseString + "\n");
            return httpResponseString;
        }
        catch(WebException webExp){
            if(webExp.Status == WebExceptionStatus.ProtocolError && webExp.Response != null){
                var httpResponse = (HttpWebResponse) webExp.Response;
                var httpResponseString = new StreamReader(httpResponse.GetResponseStream()).ReadToEnd();

                Console.WriteLine("Unsuccessful Http Response:");
                if(showResponsePayload) Console.WriteLine("Status Code: " + httpResponse.StatusCode + "\nStringified Response Object: " + httpResponseString + "\n");
                return httpResponseString;
            }
            else{
                Console.WriteLine("Could not send a successful request. Something went wrong.\n");
                return "";
            }
        }
    }
}

public static class FireWriter {
    public static async Task Write(string fileURL, string data){
        await File.WriteAllTextAsync(fileURL, data);
    }
}