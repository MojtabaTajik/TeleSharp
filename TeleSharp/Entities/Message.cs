using System;
using System.Collections.Generic;
using TeleSharp.Entities.Inline;

namespace TeleSharp.Entities
{
    public class Message
    {
        public int MessageId { get; set; }

        public User From { get; set; }
        public DateTime Date { get; set; }
        public Chat Chat { get; set; }

        public User ForwardFrom { get; set; }
        public Chat ForwardFromChat { get; set; }
        public DateTime ForwardDate { get; set; }

      
        public Message ReplyToMessage { get; set; }
        public int Edit_Date { get; set; }
        public string Text { get; set; }
        public List<Message> Entities { get; set; }
        //public Audio Audio { get; set; }
        public Audio Audio { get; set; }
        public Document Document { get; set; }
        public Game Game { get; set; }

        public List<PhotoSize> Photo { get; set; }
        public Sticker Sticker { get; set; }
        public Video Video { get; set; }
        public Voice Voice { get; set; }
        public string Caption { get; set; }

        public Contact Contact { get; set; }
        public Location Location { get; set; }

        public Venue Venue { get; set; }
        public User NewChatMember { get; set; }
        public string NewChatTitle { get; set; }
        public List<PhotoSize> NewChatPhoto { get; set; }
        public int MigrateToChatId { get; set; }
        public int MigrateFromChatId { get; set; }
        public Message PinnedMessage { get; set; }
        public InlineQuery InlineQuery { get; set; }
        public ChosenInlineResult ChosenInlineResult { get; set; }
        public User NewChatParticipant { get; set; }
        public User LeftChatParticipant { get; set; }

        public bool DeleteChatPhoto { get; set; }
        public bool GroupChatCreated { get; set; }

        public Message AsUserMessage()
        {
            Chat = null;
            return this;
        }
    }
}
