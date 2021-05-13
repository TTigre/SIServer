using System;
using System.Collections;
using System.Collections.Generic;
namespace SIServer.ViewModels
{
    public class DocumentDescriber
    {
        public ulong DocumentID{get; set;}
        public string From{get; set;}
        public string Subject{get; set;}
        public string Document{get; set;}
    }
    public class DocumentRepetitionDic
    {
        public DocumentDescriber Descriptor{get; set;}
        public Dictionary<string,uint> RepeticionesPalabras{get; set;}
    }
}