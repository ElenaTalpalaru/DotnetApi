namespace DotnetApi.Dtos
{
    public class PostToEditDto
    {
        public int PostId { get; set; }
        public string PostTitle { get; set; } = "";
        public string PostContent { get; set; } = "";
    }
}
