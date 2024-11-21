using RestWithASPNETUdemy.Model;
using System.Threading;


namespace RestWithASPNETUdemy.Services.Implementations
{
    public class PersonServiceImplementation : IPersonService
    {
        private volatile int count;

        public Person Create(Person person)
        {
            person.Id = count++;
            return person;
        }

        public Person FindById(long id)
        {
            return new Person
            {
                Id = IncrementAndGet(),
                FirstName = "Ewerton",
                LasttName = "Silva",
                Address = "Rua x, 111, bairro y - Campinas",
                Gender = "Male"
            };
        }

        public List<Person> FindAll()
        {
            List<Person> people = new List<Person>();
            for (int i = 0; i < 8; i++)
            {
                Person person = MockPerson(i);
                people.Add(person);
            }
            return people;
        }

        public Person Update(Person person)
        {
            return person;
        }

        public void Delete(long id)
        {

        }

        private Person MockPerson(int i)
        {
            return new Person
            {
                Id = IncrementAndGet(),
                FirstName = "Person Name" + i,
                LasttName = "Person LastName" + i,
                Address = "Some addrass",
                Gender = "Male"
            };
        }

        private long IncrementAndGet() { return Interlocked.Increment(ref count); }

    }
}