using RestWithASPNETUdemy.Data.Converter.Implementations;
using RestWithASPNETUdemy.Data.VO;
using RestWithASPNETUdemy.Model;
using RestWithASPNETUdemy.Repository;

namespace RestWithASPNETUdemy.Business.Implementation
{
    public class BookBusinessImplementation : IBookBusiness
    {
        private readonly IRepository<Book> _repository;
        private readonly BookConverter _converter;

        public BookBusinessImplementation(IRepository<Book> repository)
        {
            _repository = repository;
            _converter = new BookConverter();
        }

        public List<BookVO> FindAll()
        {
            return _converter.Parse(_repository.FindAll());
        }

        public BookVO? FindById(long id)
        {
            return _converter.Parse(_repository.FindById(id));
        }
        public BookVO Create(BookVO bookVO)
        {
            Book bookEntity = _converter.Parse(bookVO);
            return _converter.Parse(_repository.Create(bookEntity));
        }

        public BookVO? Update(BookVO bookVO)
        {
            Book bookEntity = _converter.Parse(bookVO);
            return _converter.Parse(_repository.Update(bookEntity));
        }

        public void Delete(long id)
        {
            _repository.Delete(id);
        }

    }
}