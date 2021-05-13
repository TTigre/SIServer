using System;
using System.Collections.Generic;
namespace SIServer.Models
{
    public class WordRepetition
    {
        public ulong DocumentID{get; set;}
        public string Word{get; set;}
        public uint Ammount{get; set;}
        public virtual Document Document{get; set;}
        public virtual RelevanciaPalabraDocumento? Relevancia{get; set;}
    }
}