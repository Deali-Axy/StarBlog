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

    public async Task<List<LinkExchange>> GetAll() {
        return await _repo.Select.ToListAsync();
    }

    public async Task<LinkExchange?> GetById(int id) {
        return await _repo.Where(a => a.Id == id).FirstAsync();
    }

    public async Task<LinkExchange> AddOrUpdate(LinkExchange item) {
        return await _repo.InsertOrUpdateAsync(item);
    }

    public async Task<LinkExchange?> SetVerifyStatus(int id, bool status) {
        var item =await GetById(id);
        if (item == null) return null;

        item.Verified = status;
        await _repo.UpdateAsync(item);


        var link = await _linkService.GetByName(item.Name);
        if (status) {
            if (link == null) {
                await _linkService.AddOrUpdate(new Link {
                    Name = item.Name,
                    Description = item.Description,
                    Url = item.Url,
                    Visible = true
                });
            }
            else {
                await _linkService.SetVisibility(link.Id, true);
            }
        }
        else {
            if (link != null) await _linkService.DeleteById(link.Id);
        }

        return await GetById(id);
    }

    public async Task<int> DeleteById(int id) {
        return await _repo.DeleteAsync(a => a.Id == id);
    }
}