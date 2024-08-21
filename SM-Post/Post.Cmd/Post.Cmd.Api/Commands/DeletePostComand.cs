using CQRS.Core.Commands;

namespace Post.Cmd.Api.Commands
{
    public class DeletePostComand : BaseCommand
    {
        public string Username  { get; set; }
    }
}
