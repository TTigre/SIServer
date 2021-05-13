using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SIServer.Models;
using SIServer.Data;
using SIServer.ViewModels;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using System.Reflection;
namespace SIServer.Services
{
    public class Metrics
    {
        public static async Task<IEnumerable<RelevanciaPalabraDocumento>> ObtenerWjs(/*IEnumerable<WordRepetition> palabrasQuery,*/ ApplicationDbContext context)
        {
            var subquery=context.WordRepetitions;
            // var subquery=from q in palabrasQuery
            //              from p in context.WordRepetitions
            //              where p.Word==q.Word
            //              select p;

            var obteniendoMaximos=from e in subquery
                                  group e by e.Word into g
                                  select new {id=g.Key, ammount=g.Max(x => x.Ammount)};
            var listamaximos=obteniendoMaximos.ToList();

            var tf=from p in subquery
                   from m in obteniendoMaximos
                   where p.Word==m.id
                   select new{documento=p.DocumentID, palabra=p.Word,peso=p.Ammount/(double)m.ammount};

            var listatf=tf.ToList();
            
            var documentosConPresencia=from p in subquery
                                       group p by p.Word into g
                                       select new {id=g.Key, ammount=g.Count()};

            var listaDocumentos=documentosConPresencia.ToList();
            
            var totalDocumentos=(double)context.Documents.Count();
            
            var idf=from d in documentosConPresencia
                    select new{id=d.id, peso=Math.Log10(totalDocumentos/(double)d.ammount)};

            var listaidf=idf.ToList();
            
            //Esta consulta termino realizandose con diccionarios internamente
            var ws=from t in listatf//tf
                   from f in listaidf//idf
                   where t.palabra==f.id
                      && t.palabra!=null
                      && f.id!=null
                   select new RelevanciaPalabraDocumento(t.documento,t.palabra,t.peso*f.peso);

            var dictf=new Dictionary<string,Dictionary<ulong,dynamic>>();
            foreach(var a in listatf)
            {
                if(!dictf.ContainsKey(a.palabra))
                {
                    dictf[a.palabra]=new Dictionary<ulong, dynamic>();
                }
                dictf[a.palabra][a.documento]=a;
            }

            var dicidf=listaidf.ToDictionary(x => x.id);

            var listaws=new List<RelevanciaPalabraDocumento>();

            foreach(string key in dicidf.Keys)
            {
                var idfActual=dicidf[key];
                
                if(dictf.ContainsKey(key))
                {
                    foreach(ulong key2 in dictf[key].Keys)
                    {
                        var tfActual=dictf[key][key2];
                        listaws.Add(new RelevanciaPalabraDocumento(tfActual.documento, idfActual.id, tfActual.peso*idfActual.peso));
                    }
                }
            }
            
            return listaws;
        }
        public static async Task<IEnumerable<RelevanciaPalabraDocumento>> WjsConsulta(IEnumerable<WordRepetition> palabrasQuery, ApplicationDbContext context)
        {
            var maximapresencia=palabrasQuery.Max(x => x.Ammount);
            var countDocumentos=context.Documents.Count();
            
            var subquery=from p in palabrasQuery
                         from wr in context.WordRepetitions
                         where p.Word==wr.Word
                         select wr;
            
            var documentosConPresencia=from wr in subquery
                                       group wr by wr.Word into g
                                       select new {id=g.Key, ammount=g.Count()};

            var idf=from d in documentosConPresencia
                    select new{id=d.id, peso=Math.Log10(countDocumentos/d.ammount)};

            double a=0.4;
            var Wiq=from p in palabrasQuery
                    from factor in idf
                    where p.Word==factor.id
                    select new RelevanciaPalabraDocumento(0,p.Word,(a +(1.0-a)*p.Ammount/maximapresencia)*factor.peso);

            return Wiq;

        }

        public static IEnumerable<ResultDescriber> SimilitudDocumentosConQuery(IEnumerable<RelevanciaPalabraDocumento> palabrasQuery, ApplicationDbContext context, int limiteDocumentos=100)
        {
            var setPalabrasQuery=palabrasQuery.ToList();
            var setPalabrasQueryWords=setPalabrasQuery.Select(x => x.Palabra).ToHashSet();
            var palabrasEnDocumentos=from p in context.RelevanciaPalabraDocumentos
                                     where setPalabrasQueryWords.Contains(p.Palabra)
                                     select p;
            dynamic lista;

            var sumaAbajoPorDocumento=from p in context.RelevanciaPalabraDocumentos
                                      group p by p.DocumentID into g
                                      select new{id=g.Key, suma=Math.Sqrt(g.Sum(x => x.Relevancia*x.Relevancia))};
            
            var listasumaAbajoPorDocumento=sumaAbajoPorDocumento.ToDictionary(x => x.id);

            var sumaAbajoQuery=Math.Sqrt(palabrasQuery.Sum(x => x.Relevancia*x.Relevancia));

            var setpalbrasEnDocumentos=palabrasEnDocumentos.ToList();

            var setpalabrasQuery=palabrasQuery.ToList();

            var dicTemp=setpalabrasQuery.ToDictionary(x => x.Palabra);

            var preparteArriba=from p in setpalbrasEnDocumentos//palabrasEnDocumentos
                            // from q in setpalabrasQuery
                            // where p.Palabra==Queryable.Palabra
                            where dicTemp.ContainsKey(p.Palabra)
                            select new {id=p.DocumentID, peso=p.Relevancia*dicTemp[p.Palabra].Relevancia};
                            // select new {id=p.DocumentID, peso=p.Relevancia*q.Relevancia};
            
            lista=preparteArriba.ToList();

            var parteArriba=from p in preparteArriba
                            group p by p.id into g
                            select new {id=g.Key, suma=g.Sum(x => x.peso)};
                            // select g;

            // var dicArriba=new Dictionary<ulong,double>();
            // foreach(var element in parteArriba)
            // {
            //     dicArriba[element.Key]=element.Sum(x => x.peso);
            // }

            var listaparteArriba=parteArriba.ToDictionary(x => x.id);

            var listaFinal=new List<ResultDescriber>();

            foreach(ulong key in listaparteArriba.Keys)
            {
                if(listasumaAbajoPorDocumento.ContainsKey(key))
                {
                    listaFinal.Add(new ResultDescriber(key, listaparteArriba[key].suma/(listasumaAbajoPorDocumento[key].suma*sumaAbajoQuery)));
                }
            }

            // var todo=from up in parteArriba
            //          from d in sumaAbajoPorDocumento
            //          where up.id==d.id
            //          select new ResultDescriber(up.id, up.suma/(d.suma*sumaAbajoQuery));
            //         //  where dicArriba.ContainsKey(d.id)
            //         //  select new ResultDescriber(d.id,dicArriba[d.id]/(d.suma*sumaAbajoQuery));
            
            // lista=todo.ToList();

            var result=listaFinal.OrderByDescending(x => x.Similitud).Take(limiteDocumentos);

            return result;
        }
    }
}