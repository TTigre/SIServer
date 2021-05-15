using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SIServer.Models;
using SIServer.Data;
using System.IO;
using SIServer.Services;
using System.Collections.Concurrent;

namespace SIServer.Controllers
{
    [ApiController]
    [Route("api/document/")]
    public class DoucumentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoucumentController(ApplicationDbContext context)
        {
            _context=context;
        }
        [HttpGet("Relevancia")]
        public async Task<IActionResult> CalculaRelevancia()
        {
            var resultado=(await Metrics.ObtenerWjs(_context)).ToList();
            await _context.RelevanciaPalabraDocumentos.AddRangeAsync(resultado);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerDocumento(ulong id)
        {
            var documentoLINQ=from d in _context.Documents
                              where d.DocID==id
                              select d;
            var documento=documentoLINQ.FirstOrDefault();
            if(documento==null)
            {
                return NotFound();
            }
            var st=new FileStream(documento.Ruta,FileMode.Open);
            return File(st,"text/plain");
            // throw new FileNotFoundException();
        }
        [HttpGet("Relevancia/Documentos/{query}")]
        public async Task<IActionResult> ObtenerDocumentosRelevantes(string query)
        {
            var describer=Preproccess.PreproccessQuery(query);

            var listaRepeticiones=new List<WordRepetition>();

            foreach(string key in describer.RepeticionesPalabras.Keys)
            {
                var repeticion=new WordRepetition();
                repeticion.DocumentID=0;
                repeticion.Word=key;
                repeticion.Ammount=describer.RepeticionesPalabras[key];
                listaRepeticiones.Add(repeticion);
            }

            var wQuery=await Metrics.WjsConsulta(listaRepeticiones,_context);

            var listawQuery=wQuery.ToList();

            var respuesta=Metrics.SimilitudDocumentosConQuery(wQuery, _context);

            var respuestaFinal=from r in respuesta
                               from d in _context.Documents
                               where r.DocumentID==d.DocID
                               select d;

            return Ok(respuestaFinal);
        }

        [HttpGet("Relevancia/Query/{query}")]
        public async Task<IActionResult> CalculaRelevanciaQuery(string query)
        {
            var describer=Preproccess.PreproccessQuery(query);
            
            var listaRepeticiones=new List<WordRepetition>();

            foreach(string key in describer.RepeticionesPalabras.Keys)
            {
                var repeticion=new WordRepetition();
                repeticion.DocumentID=0;
                repeticion.Word=key;
                repeticion.Ammount=describer.RepeticionesPalabras[key];
                listaRepeticiones.Add(repeticion);
            }
            var respuesta=await Metrics.WjsConsulta(listaRepeticiones,_context);
            return Ok(respuesta);
        }


        [HttpGet("Carga/{directory}")]
        public async Task<IActionResult> CargaLosDocumentos(string directory)
        {
            var paths=DocumentReader.GetDocPaths(directory);
            foreach(string s in paths)
            {
                if(s.EndsWith("51134"))
                {
                    Console.WriteLine("Aqui");
                }
                Console.WriteLine(s);
                var st=new StreamReader(s);
                string doc=await st.ReadToEndAsync();
                st.Close();
                var describer=Preproccess.Preproccess1(0,doc);

                var documento=new Document();
                documento.Ruta=s;
                documento.From=describer.Descriptor.From;
                documento.Subject=describer.Descriptor.Subject;
                await _context.AddAsync(documento);
                await _context.SaveChangesAsync();

                var listaRepeticiones=new List<WordRepetition>();

                foreach(string key in describer.RepeticionesPalabras.Keys)
                {
                    var repeticion=new WordRepetition();
                    repeticion.DocumentID=documento.DocID;
                    repeticion.Word=key;
                    repeticion.Ammount=describer.RepeticionesPalabras[key];
                    listaRepeticiones.Add(repeticion);
                }
                await _context.AddRangeAsync(listaRepeticiones);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

    }
}
