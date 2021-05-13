using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SIServer.Models;
using SIServer.Data;
using System.Linq.Expressions;
using System.Collections.Concurrent;
namespace SIServer.Services
{
    public class Metrics
    {
        public async Task<IEnumerable<RelevanciaPalabraDocumento>> ObtenerWjs(IEnumerable<WordRepetition> palabrasQuery, ApplicationDbContext context)
        {
            var subquery=from q in palabrasQuery
                         from p in context.WordRepetitions
                         where p.Word==q.Word
                         select p;

            var obteniendoMaximos=from e in subquery
                                  group e by e.Word into g
                                  select new {id=g.Key, ammount=g.Max(x => x.Ammount)};

            var tf=from p in subquery
                   from m in obteniendoMaximos
                   where p.Word==m.id
                   select new{documento=p.DocumentID, palabra=p.Word,peso=p.Ammount/(double)m.ammount};
            
            var documentosConPresencia=from p in subquery
                                       group p by p.DocumentID into g
                                       select new {id=g.Key, ammount=g.Count()};
            
            var totalDocumentos=(double)context.Documents.Count();
            
            var idf=from d in documentosConPresencia
                    select new{id=d.id, peso=Math.Log10(totalDocumentos/d.ammount)};
            
            var ws=from t in tf
                   from f in idf
                   where t.documento==f.id
                   select new RelevanciaPalabraDocumento(t.documento,t.palabra,t.peso*f.peso);
            
            return ws;
        }
        public async Task<IEnumerable<RelevanciaPalabraDocumento>> WjsConsulta(IEnumerable<WordRepetition> palabrasQuery, ApplicationDbContext context)
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
    }
}