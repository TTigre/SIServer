using System;
using System.Collections;
using System.Collections.Generic;
namespace SIServer.ViewModels
{
    public class ResultDescriber
    {
        public ResultDescriber()
        {
            
        }
        public ResultDescriber(ulong DocumentID, double Similitud)
        {
            this.DocumentID=DocumentID;
            this.Similitud=Similitud;
        }
        public ulong DocumentID{get; set;}
        public double Similitud{get; set;}
    }
}