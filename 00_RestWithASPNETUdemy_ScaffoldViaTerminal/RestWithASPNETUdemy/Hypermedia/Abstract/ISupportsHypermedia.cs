using RestWithASPNETUdemy.Hypermedia;

namespace RestWithASPNETUdemy.Hypermedia.Abstract
{
    public interface ISupportsHypermedia
    {
        List<HyperMediaLink> links { get; set; }
    }
}