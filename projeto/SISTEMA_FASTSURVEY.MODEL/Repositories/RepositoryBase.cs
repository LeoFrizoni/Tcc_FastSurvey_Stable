using Microsoft.EntityFrameworkCore;
using SISTEMA_FASTSURVEY.MODEL.Interfaces;
using SISTEMA_FASTSURVEY.MODEL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SISTEMA_FASTSURVEY.MODEL.Repositories
{
    public class RepositoryBase<T> : IRepositoryBase<T>, IDisposable where T : class
    {
        protected readonly FastSurveyContext _context;
        protected readonly bool _saveChanges;

        public RepositoryBase(FastSurveyContext context, bool saveChanges)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _saveChanges = saveChanges;
        }

        public T Incluir(T obj)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));

            _context.Set<T>().Add(obj);
            if (_saveChanges)
            {
                _context.SaveChanges();
            }
            return obj;
        }

        public async Task<T> IncluirAsync(T obj)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));

            await _context.Set<T>().AddAsync(obj); // <-- agora realmente aguarda
            if (_saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            return obj;
        }

        public T Alterar(T obj)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));

            _context.Entry(obj).State = EntityState.Modified;
            if (_saveChanges)
            {
                _context.SaveChanges();
            }
            return obj;
        }

        public async Task<T> AlterarAsync(T obj)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));

            _context.Entry(obj).State = EntityState.Modified;
            if (_saveChanges)
            {
                await _context.SaveChangesAsync();
            }
            return obj;
        }

        public void Excluir(T obj)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));

            _context.Set<T>().Remove(obj);
            if (_saveChanges)
            {
                _context.SaveChanges();
            }
        }

        public void Excluir(params object[] variavel)
        {
            var obj = _context.Set<T>().Find(variavel);
            if (obj is null) return; // nada a excluir

            Excluir(obj);
        }

        public async Task ExcluirAsync(T obj)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));

            _context.Set<T>().Remove(obj);
            if (_saveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task ExcluirAsync(params object[] variavel)
        {
            var obj = await _context.Set<T>().FindAsync(variavel);
            if (obj is null) return; // nada a excluir

            await ExcluirAsync(obj);
        }

        public T SelecionarChave(params object[] variavel)
        {
            return _context.Set<T>().Find(variavel);
        }

        public async Task<T> SelecionarChaveAsync(params object[] variavel)
        {
            return await _context.Set<T>().FindAsync(variavel);
        }

        public List<T> SelecionarTodos()
        {
            return _context.Set<T>().ToList();
        }

        public async Task<List<T>> SelecionarTodosAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public void Dispose()
        {
            // NO-OP: o DbContext é descartado pelo DI no fim do escopo HTTP.
            // Se você estiver instanciando este repositório manualmente (fora do DI),
            // e quiser descartar o contexto aqui, substitua por: _context.Dispose();
        }
    }
}
