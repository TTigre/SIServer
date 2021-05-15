using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
namespace SIServer.Models
{
    public class Document
    {
        public Document()
        {
            From="";
            Subject="";
            Ruta="";
        }
        public ulong DocID{get; set;}
        public string From{get; set;}
        public string Subject{get; set;}
        [JsonIgnore]
        public string Ruta{get; set;}
        [JsonIgnore]
        public virtual ICollection<WordRepetition> Palabras{get; set;}
        [JsonIgnore]
        public virtual ICollection<RelevanciaPalabraDocumento> Relevancias{get; set;}
    }
}