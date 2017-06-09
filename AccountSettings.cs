  using AppName.Web.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppName.Web.Domain.Account;
using AppName.Web.Models.Requests.Account;
using System.Data.SqlClient;
using System.Data;
using AppName.Data;
using AppName.Web.Domain.Newsletter.MembersResponse;
using Newtonsoft.Json;
using AppName.Web.Enums;


namespace AppName.Web.Services
{
    public class AccountSettingsService : BaseService, IAccountSettingsService
    {
        IAccountsService _accountSrv = null;

        public AccountSettingsService(IAccountsService accountSrvInject)
        {
            _accountSrv = accountSrvInject;
        }
  
  public bool Insert(AccountSettingRequest data)
        {
            if (Get(data) != null)
            {
                Update(data);
                return false;
            }

            short IsSuccessful = 0;
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.AccountSettings_Insert",
                inputParamMapper: delegate (SqlParameterCollection param)
                {
                    param.AddWithValue("@UserId", data.UserId);
                    param.AddWithValue("@SettingId", data.SettingId);
                    param.AddWithValue("@Value", data.Value);
                    param.Add(new SqlParameter { ParameterName = "IsSuccessful", Value = SqlDbType.Bit, Direction = ParameterDirection.Output });
                },
                returnParameters: delegate (SqlParameterCollection param)
                {
                    short.TryParse(param["IsSuccessful"].Value.ToString(), out IsSuccessful);
                });
            if (IsSuccessful == 1) { return true; }
            return false;
        }
        
        
           public string GetValue(string handle, int settingId = 6)
        {
            string value = null;
            DataProvider.ExecuteCmd(GetConnection, "dbo.AccountSettings_GetSettingValueByHandle",
                inputParamMapper: delegate (SqlParameterCollection param)
                {
                    param.AddWithValue("@Handle", handle);
                    param.AddWithValue("@SettingId", settingId);
                },
                map: delegate (IDataReader reader, short set)
                {
                    value = reader.GetSafeString(0);
                }
                );
            return value;
        }
     }
 }
