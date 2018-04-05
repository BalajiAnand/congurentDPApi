using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using drportal.Enums;
using drportal.Helpers;
using drportal.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace drportal.Repositories
{
    public class UserRepository
    {
        private DBHelper dbHelper;

        public UserRepository(string connectionString)
        {
            this.dbHelper = new DBHelper(connectionString);
        }

        public UserModel ToUser(DataRow dataRow)
        {
            var userModel = new UserModel()
            {
                UserGuid = Guid.Parse(Convert.ToString(dataRow["UserGuid"])),
                Name = Convert.ToString(dataRow["Name"]),
                Email = Convert.ToString(dataRow["Email"]),
                DOB = Convert.ToDateTime(dataRow["DOB"]),
                Gender = (Gender)Convert.ToInt32(dataRow["Gender"])
            };

            if (dataRow["ForgotPasswordHash"] != null)
            {
                userModel.ForgotPasswordHash = Convert.ToString(dataRow["ForgotPasswordHash"]);
            }
            if (!String.IsNullOrEmpty(Convert.ToString(dataRow["ForgotPasswordExpiry"])))
            {
                userModel.ForgotPasswordExpiry = Convert.ToDateTime(dataRow["ForgotPasswordExpiry"]);
            }

            return userModel;
        }

        public bool RegisterUser(RegistrationRequestModel registrationRequest)
        {
            var encryptedPassword = Crypto.GenerateSHA256String(registrationRequest.Password);
            var queryParams = new Dictionary<string, object>();
            queryParams.Add("@UserGuid", Guid.NewGuid());
            queryParams.Add("@Name", registrationRequest.Name);
            queryParams.Add("@Email", registrationRequest.Email);
            queryParams.Add("@DOB", registrationRequest.DOB);
            queryParams.Add("@Password", encryptedPassword);
            queryParams.Add("@Gender", registrationRequest.Gender);
            return this.dbHelper.ExecuteNonQuery(true, "uspRegisterUser", queryParams) > 0;
        }

        public UserModel AuthenticateUser(LoginRequestModel loginRequest)
        {
            var encryptedPassword = Crypto.GenerateSHA256String(loginRequest.Password);
            var queryParams = new Dictionary<string, object>();
            queryParams.Add("@Email", loginRequest.Email);
            queryParams.Add("@Password", encryptedPassword);
            var response = this.dbHelper.ExecuteQuery(true, "uspAuthenticateUser", queryParams);

            if (response != null && response.Tables.Count > 0)
            {
                var rows = response.Tables[0].Rows;

                if (rows.Count == 1)
                {
                    return this.ToUser(rows[0]);
                }
            }

            return null;
        }

        public bool UpdateForgotPasswordHash(Guid UserGuid, string hash)
        {
            var queryParams = new Dictionary<string, object>();
            queryParams.Add("@UserGuid", UserGuid);
            queryParams.Add("@Hash", hash);
            return this.dbHelper.ExecuteNonQuery(true, "uspSetForgotPasswordHash", queryParams) > 0;
        }

        public bool ChangePassword(ChangePasswordRequestModel changePasswordRequest) {
            var queryParams = new Dictionary<string, object>();
            var encryptedPassword = Crypto.GenerateSHA256String(changePasswordRequest.NewPassword);
            queryParams.Add("@UserGuid", changePasswordRequest.UserGuid);
            queryParams.Add("@Password", encryptedPassword);
            queryParams.Add("@PasswordHash", changePasswordRequest.ChangePasswordHash);
            return this.dbHelper.ExecuteNonQuery(true, "uspChangePassword", queryParams) > 0;
        }

        public UserModel GetUser(Guid UserGuid) {
            return GetUser(UserGuid, null);
        }

        public UserModel GetUser(string email)
        {
            return GetUser(null, email);
        }

        private UserModel GetUser(Guid? UserGuid, string email)
        {
            var queryParams = new Dictionary<string, object>();
            queryParams.Add("@UserGuid", UserGuid);
            queryParams.Add("@Email", email);
            var response = this.dbHelper.ExecuteQuery(true, "uspGetUser", queryParams);

            if (response != null && response.Tables.Count > 0)
            {
                var rows = response.Tables[0].Rows;

                if (rows.Count == 1)
                {
                    return this.ToUser(rows[0]);
                }
            }

            return null;
        }

        public bool SocialRegistration(SocialRequestModel socialRequestModel)
        {
            string uid = string.Empty;
            string name = string.Empty;
            string email = null;
            string dob = string.Empty;
            Gender gender = Gender.Unspecified;

            switch (socialRequestModel.Type)
            {
                case "Facebook":

                    string url = string.Format("https://graph.facebook.com/me?fields=id,email,birthday,name,gender&access_token={0}",
                     socialRequestModel.Token);
                    oAuthFacebook objFbCall = new oAuthFacebook();
                    string JSONInfo = objFbCall.WebRequest(oAuthFacebook.Method.GET, url, string.Empty);

                    JObject Job = JObject.Parse(JSONInfo);
                    JToken Jdata = Job.Root;

                    if (Jdata.HasValues)
                    {
                        uid = (string)Jdata.SelectToken("id");
                        email = (string)Jdata.SelectToken("email");
                        dob = (string)Jdata.SelectToken("birthday");
                        name = (string)Jdata.SelectToken("name");

                        if (((string)Jdata.SelectToken("gender")).Equals("male", StringComparison.InvariantCultureIgnoreCase))
                        {
                            gender = Gender.Male;
                        }
                        else if (((string)Jdata.SelectToken("gender")).Equals("female", StringComparison.InvariantCultureIgnoreCase))
                        {
                            gender = Gender.Female;
                        }
                        else
                        {
                            gender = Gender.Unspecified;
                        }
                    }
                    break;

                case "Google":

                    string googleUrl = string.Format("https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={0}",
                                         socialRequestModel.Token);
                    WebRequest request = WebRequest.Create(googleUrl);
                    request.Credentials = CredentialCache.DefaultCredentials;
                    WebResponse response = request.GetResponse();
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();
                    reader.Close();
                    response.Close();
                    GoogleApiInfoModel googleApiInfo = JsonConvert.DeserializeObject<GoogleApiInfoModel>(responseFromServer);
                    uid = googleApiInfo.aud;
                    email = googleApiInfo.email;
                    dob = "";
                    name = googleApiInfo.name;
                    gender = Gender.Unspecified;
                    break;
            }

            if (email != null)
            {
                RegisterUser(new RegistrationRequestModel()
                {
                    UserGuid = Guid.NewGuid(),
                    Name = name,
                    DOB = dob,
                    Email = email,
                    Password = "temp123",
                    Gender = gender
                });
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}