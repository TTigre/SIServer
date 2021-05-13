using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SIServer.Models;
using SIServer.ViewModels;
using Porter2Stemmer;
namespace SIServer.Services
{
    public static class Preproccess
    {
        public static string[] Stem(string[] document)
        {
            var st=new EnglishPorter2Stemmer();
            var lista=new List<string>();
            foreach(string s in document)
            {
                var stemmed=st.Stem(s);
                lista.Add(stemmed.Value);
            }
            return lista.ToArray();
        }
        public static DocumentDescriber GenerateDocumentDescription(ulong id,string document)
        {
            var fromHeader="From: ";
            var subjectHeader="Subject: ";

            var indiceFrom=document.IndexOf(fromHeader);
            var indiceFromFinal=document.IndexOf('\n',indiceFrom);

            var indiceSubject=document.IndexOf(subjectHeader);
            var indiceSubjectFinal=document.IndexOf('\n',indiceSubject);

            if(indiceSubjectFinal==-1)
            {
                Console.WriteLine("Aquí");
            }

            var longitudFrom=indiceFromFinal-(indiceFrom+fromHeader.Length);
            var longitudSubject=indiceSubjectFinal-(indiceSubject+subjectHeader.Length);
            if (indiceSubject<0)
            {
                longitudSubject=indiceSubjectFinal-(indiceSubject+subjectHeader.Length);
            }

            var From=document.Substring(indiceFrom+fromHeader.Length,longitudFrom);
            var Subject=document.Substring(indiceFrom+subjectHeader.Length,longitudSubject);

            var descriptor = new DocumentDescriber
            {
                DocumentID = id,
                From = From,
                Subject = Subject,
                Document = document.Substring(indiceSubjectFinal + 1)
            };

            return descriptor;
        }
        public static string[] Split(string document)
        {
            string nuevoDoc="";
            char[] separators={
                ' ',
                '\n',
                ',',
                ';',
                '>',
                '<',
                '-',
                '!',
                '_',
                '?',
                '/',
                '\\',
                '(',
                ')',
                '|',
                '\t',
                '.',
                ':',
                '*',
                '"',
                '\'',
            };
            var set=new HashSet<char>(separators);
            nuevoDoc+=document[0];
            for(int i=1; i<document.Length-1; i++)
            {
                if(document[i]=='.'&&!set.Contains(document[i+1])&&!set.Contains(document[i-1]))
                {
                    continue;
                }
                nuevoDoc+=document[i];
            }
            nuevoDoc+=document[document.Length-1];

            return nuevoDoc.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] Clean(string[] document)
        {
            string[] eliminables={
                "the",
                "a",
                "an",
                "of",
                "and",
                "or",
                "no",
                "not",
                "with",
                "which",
                "&",
            };
            var set=new HashSet<string>(eliminables);
            var lista=new List<string>();
            foreach(string element in document)
            {
                if(set.Contains(element.ToLower()))
                {
                    continue;
                }
                lista.Add(element.ToLower());
            }
            return lista.ToArray();
        }

        public static string[] SplitStemClean(string document)
        {
            var splitted=Split(document);
            var cleanned=Clean(splitted);
            var stemmed=Stem(cleanned);
            return stemmed;
        }
        
        public static DocumentRepetitionDic GenerateDocumentRepetitionDic(DocumentDescriber document)
        {
            Dictionary<string,uint> Repeticiones=new Dictionary<string, uint>();
            foreach(var s in Preproccess.SplitStemClean(document.From))
            {
                if(Repeticiones.ContainsKey(s))
                {
                    Repeticiones[s]++;
                }
                else
                {
                    Repeticiones[s]=1;
                }
            }
            foreach(var s in Preproccess.SplitStemClean(document.Subject))
            {
                if(Repeticiones.ContainsKey(s))
                {
                    Repeticiones[s]++;
                }
                else
                {
                    Repeticiones[s]=1;
                }
            }
            foreach(var s in Preproccess.SplitStemClean(document.Document))
            {
                if(Repeticiones.ContainsKey(s))
                {
                    Repeticiones[s]++;
                }
                else
                {
                    Repeticiones[s]=1;
                }
            }

            var documentRepetitionDic = new DocumentRepetitionDic
            {
                Descriptor = document,
                RepeticionesPalabras = Repeticiones
            };

            return documentRepetitionDic;
        }

        public static DocumentRepetitionDic UncaseDocumentRepetitionDic(DocumentRepetitionDic document)
        {
            var Repeticiones=new Dictionary<string, uint>();
            foreach(var llave in document.RepeticionesPalabras.Keys)
            {
                if(llave.Length==0)
                {
                    continue;
                }

                var llaveUncased=llave.ToLower();
                if(Repeticiones.ContainsKey(llaveUncased))
                {
                    Repeticiones[llaveUncased]+=document.RepeticionesPalabras[llave];
                }
                else
                {
                    Repeticiones[llaveUncased]=document.RepeticionesPalabras[llave];
                }
            }

            var documentRepetitionDic = new DocumentRepetitionDic
            {
                Descriptor = document.Descriptor,
                RepeticionesPalabras = Repeticiones
            };

            return documentRepetitionDic;
        }
        public static DocumentRepetitionDic Preproccess1(ulong id, string document)
        {
            var desc=GenerateDocumentDescription(id, document);
            var docDic=GenerateDocumentRepetitionDic(desc);
            // docDic=UncaseDocumentRepetitionDic(docDic);
            return docDic;
        }
        public static DocumentRepetitionDic PreproccessQuery(string document)
        {
            ulong id=0;
            string nuevoDocument="From:  \n    Subject:  \n\n    "+document;
            var desc=GenerateDocumentDescription(id, nuevoDocument);
            var docDic=GenerateDocumentRepetitionDic(desc);
            // docDic=UncaseDocumentRepetitionDic(docDic);
            return docDic;
        }
    }
}