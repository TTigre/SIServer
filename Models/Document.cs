using System;
using System.Collections;
using System.Collections.Generic;
namespace SIServer.Models
{
    public class Document
    {
        public ulong DocID{get; set;}
        public string Ruta{get; set;}
        public virtual ICollection<WordRepetition> Palabras{get; set;}
        public virtual ICollection<RelevanciaPalabraDocumento> Relevancias{get; set;}
    }
}