using SRTFormat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRTFormat.DBAccess
{
    public class RuleDataAccess
    {

        public void AddRule(RuleModel model)
        {
            string sql = @"insert into Rules(
                            Name
                            ,RegValue
                            ,ReplaceValue
                            ,Desc
                            CreateDate
                         ) values (
                            @Name
                            ,@RegValue
                            ,@ReplaceValue
                            ,@Desc
                            ,@CreateDate
                         )";
            object[] sqlParams = new object[]
            {
                model.Name
                ,model.RegValue
                ,model.ReplaceValue
                ,model.Desc
                ,model.CreateDate.ToString("yyyy-MM-dd HH:mm:ss")
            };
            SQLiteHelper.Instance.ExecuteNonQuery(sql, sqlParams);
        }

        public bool ExistRule(RuleModel model)
        {
            string sql = @"select Id
                            ,Name
                            ,RegValue
                            ,ReplaceValue
                            ,Desc
                            ,CreateDate from Rules
                           where RegValue = @RegValue";
            object[] sqlParams = new object[]
            {
                model.RegValue
            };
            var result = SQLiteHelper.Instance.ExecuteScalar(sql, sqlParams);

            return Convert.ToBoolean(result);
        }

        public List<RuleModel> GetRules()
        {
            string sql = @"select Id
                            ,Name
                            ,RegValue
                            ,ReplaceValue
                            ,Desc
                            ,CreateDate
                          from Rules";
            List<RuleModel> rules = new List<RuleModel>();
            using (var reader = SQLiteHelper.Instance.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    rules.Add(new RuleModel()
                    {
                        Id = Convert.ToInt32(reader["Id"].ToString()),
                        Name = reader["Name"].ToString(),
                        RegValue = reader["RegValue"].ToString(),
                        ReplaceValue = reader["ReplaceValue"].ToString(),
                        Desc = reader["Desc"].ToString(),
                        CreateDate = Convert.ToDateTime(reader["CreateDate"])
                    });
                }
            }

            return rules;
        }

        public List<RuleModel> GetCurRules()
        {
            string sql = @"select Id
                            ,Name
                            ,RegValue
                            ,ReplaceValue
                            ,Desc
                            ,CreateDate
                          from Rules
                          where Id in (
                            select RuleId from RuleTag
                            inner join Tags on RuleTag.TagId = Tags.Id
                            where
                              Tags.TagStatus = 1)";
            
            List<RuleModel> rules = new List<RuleModel>();
            using (var reader = SQLiteHelper.Instance.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    rules.Add(new RuleModel()
                    {
                        Id = Convert.ToInt32(reader["Id"].ToString()),
                        Name = reader["Name"].ToString(),
                        RegValue = reader["RegValue"].ToString(),
                        ReplaceValue = reader["ReplaceValue"].ToString(),
                        Desc = reader["Desc"].ToString(),
                        CreateDate = Convert.ToDateTime(reader["CreateDate"])
                    });
                }
            }
            return rules;
        }

        public void AddToTag(RuleModel model, int tagId)
        {
            string sql = @"insert into RuleTag (
                            TagId
                            ,RuleId
                        ) values (
                            @TagId
                            ,@RuleId
                        )";
            object[] sqlParams = new object[]
            {
                model.Id
                ,tagId
            };
            SQLiteHelper.Instance.ExecuteNonQuery(sql, sqlParams);
        }
    }
}
