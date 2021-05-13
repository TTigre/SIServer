using System;
namespace SIServer.Models
{
    public class RelevanciaPalabraDocumento
    {
        public RelevanciaPalabraDocumento()
        {
            
        }
        public RelevanciaPalabraDocumento(ulong DocumentID, string Palabra, double Relevancia)
        {
            this.DocumentID=DocumentID;
            this.Palabra=Palabra;
            this.Relevancia=Relevancia;
        }
            public ulong DocumentID{get; set;}
            public string Palabra{get; set;}
            public double Relevancia{get; set;}
            public virtual Document Documento{get; set;}
            public virtual WordRepetition Repetition{get; set;}
    }
}