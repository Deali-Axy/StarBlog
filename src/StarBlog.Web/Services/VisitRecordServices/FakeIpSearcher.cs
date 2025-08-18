﻿using System.Net;
using IP2Region.Net.Abstractions;

namespace StarBlog.Web.Services.VisitRecordServices;

public class FakeIpSearcher : ISearcher {
    private const string FakeResult = "0|0|0|0|0";
    
    public string? Search(string ipStr) => FakeResult;

    public string? Search(IPAddress ipAddress) => FakeResult;

    public string? Search(uint ipAddress) => FakeResult;

    public int IoCount => 0;
}