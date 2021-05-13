using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
namespace SIServer.Services
{
    public static class DocumentReader
    {
        public static IEnumerable<string> GetDocPaths(string Path)
        {
            Queue<string> cola=new Queue<string>();
            cola.Enqueue(Path);

            while(cola.Count>0)
            {
                string directorioActual=cola.Dequeue();
                foreach(string s in Directory.EnumerateFiles(directorioActual))
                {
                    yield return s;
                }
                foreach(string s in Directory.EnumerateDirectories(directorioActual))
                {
                    cola.Enqueue(s);
                }
            }
        }
    }
}