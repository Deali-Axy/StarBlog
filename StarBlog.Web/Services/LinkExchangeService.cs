using FreeSql;
using StarBlog.Data.Models;

namespace StarBlog.Web.Services;

public class LinkExchangeService {
    private readonly IBaseRepository<LinkExchange> _repo;
    private readonly LinkService _linkService;

    public LinkExchangeService(IBaseRepository<LinkExchange> repo, LinkService linkService) {
        _repo = repo;
        _linkService = linkService;
    }

    public List<LinkExchange> GetAll() {
        return _repo.Select.ToList();
    }

    public LinkExchange? GetById(int id) {
        return _repo.Where(a => a.Id == id).First();
    }

    public LinkExchange AddOrUpdate(LinkExchange item) {
        return _repo.InsertOrUpdate(item);
    }

    public LinkExchange? SetVerifyStatus(int id, bool status) {
        var item = GetById(id);
        if (item == null) return null;

        item.Verified = status;
        _repo.Update(item);


        var link = _linkService.GetByName(item.Name);
        if (status) {
            if (link == null) {
                _linkService.AddOrUpdate(new Link {
                    Name = item.Name,
                    Description = item.Description,
                    Url = item.Url,
                    Visible = true
                });
            }
            else {
                _linkService.SetVisibility(link.Id, true);
            }
        }
        else {
            if (link != null) _linkService.DeleteById(link.Id);
        }

        return GetById(id);
    }

    public int DeleteById(int id) {
        return _repo.Delete(a => a.Id == id);
    }
}