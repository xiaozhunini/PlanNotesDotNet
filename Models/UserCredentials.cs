namespace PlanNoteServer.Models
{
    /// <summary>
    /// 用户登录凭证表（用于账号密码登录体系，与微信 Users 表 1:1 可选关联）
    /// 对应数据库表：user_credentials
    /// 密码使用 bcrypt 算法加密后存入 PasswordHash 字段
    /// </summary>
    public class UserCredentials
    {
        /// <summary>
        /// 主键（BIGINT，自增）
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// 关联 users 表用户 ID（BIGINT，1 个用户最多 1 组账号密码，字段带唯一索引）
        /// </summary>
        public long UserID { get; set; }

        /// <summary>
        /// 登录账号（唯一，不区分大小写？这里存原值，业务层自行处理大小写统一）
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 加密后的密码（使用 bcrypt 算法，bcrypt 输出约 60 字符，字段预留 128 长度）
        /// 格式示例：$2a$12$R9h/cIPz0gygWXr/3JtGQO...
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// 最后登录 IP（IPv4 最长 15 字符、IPv6 最长 45 字符，长度预留 45）
        /// </summary>
        public string? LastLoginIP { get; set; }

        // 导航属性

        /// <summary>
        /// 导航属性：关联的用户信息（Include 查询可直接拿到用户详情）
        /// </summary>
        public Users? User { get; set; }
    }
}
