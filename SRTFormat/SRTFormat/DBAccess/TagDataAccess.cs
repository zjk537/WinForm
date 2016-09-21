using SRTFormat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRTFormat.DBAccess
{
    public class TagDataAccess
    {

        public void AddTag(string tagName)
        {
            string sql = @"insert into Tags （
                            TagName
                            ,TagStatus
                            ,CreateDate
                        ) values (
                            @TagName
                            ,@TagStatus
                            ,@CreateDate
                        )";
            object[] sqlParams = new object[]
            {
                tagName
                ,1
                ,DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            string updateSql = @"update Tags set TagStatus = 0 where TagStatus = 1";
            SQLiteHelper.Instance.ExecuteNonQuery(updateSql);
            SQLiteHelper.Instance.ExecuteNonQuery(sql, sqlParams);
        }

        public List<TagModel> GetTags()
        {
            string sql = @"select Id
                            ,TagName
                            ,TagStatus
                            ,CreateDate
                         from Tags";
            List<TagModel> tags = new List<TagModel>();
            using (var reader = SQLiteHelper.Instance.ExecuteReader(sql))
            {
                tags.Add(new TagModel()
                {
                    Id = Convert.ToInt32(reader["Id"].ToString()),
                    TagName = reader["TagName"].ToString(),
                    TagStatus = Convert.ToBoolean(reader["TagStatus"]),
                    CreateDate = Convert.ToDateTime(reader["CreateDate"])
                });
            }
            return tags;
        }
    }
}
