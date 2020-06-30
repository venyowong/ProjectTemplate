namespace ProjectTemplate
{
    public class Configuration
    {
        public LoggingConfig Logging{get;set;}

        public RedisConfig Redis{get;set;}
    }

    public class LoggingConfig
    {
        public SerilogConfig Serilog{get;set;}
    }

    public class SerilogConfig
    {
        public string File{get;set;}
    }

    public class RedisConfig
    {
        public string ConnectionString{get;set;}
    }
}