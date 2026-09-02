namespace PlanNoteServer.Configuration
{
    /// <summary>
    /// Redis 连接配置（在 Program.cs 中通过 IOptions 模式绑定 appsettings.json 的 Redis 节）
    /// </summary>
    public class RedisSettings
    {
        /// <summary>
        /// 连接字符串（如 localhost:6379）
        /// </summary>
        public string ConnectionString { get; set; } = "localhost:6379";

        /// <summary>
        /// 实例名前缀（用于所有 key 的统一命名空间，避免与其他业务冲突）
        /// </summary>
        public string InstanceName { get; set; } = "PlanNotes:";

        /// <summary>
        /// 默认数据库（0-15）
        /// </summary>
        public int DefaultDatabase { get; set; } = 0;

        /// <summary>
        /// 连接失败时是否抛异常（开发环境可设为 false 容错）
        /// </summary>
        public bool AbortOnConnectFail { get; set; } = false;

        /// <summary>
        /// 是否启用 SSL
        /// </summary>
        public bool Ssl { get; set; } = false;
    }
}
