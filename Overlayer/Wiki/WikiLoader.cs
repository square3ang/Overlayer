using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Overlayer.Wiki;

public class WikiLoader {
    private bool IsLoading = false;
    private bool failed = false;
    private event Action<List<WikiData>> OnLoaded = delegate { };

    public WikiLoader(Action<List<WikiData>> listener) => OnLoaded += listener;

    public bool IsLoadingNow() => IsLoading;
    public bool IsFailed() => failed;

    public async Task LoadAsync(string folderPath) {
        if(IsLoading) {
            return;
        }
        IsLoading = true;
        string[] files;

        try {
            files = Directory.GetFiles(folderPath, "*.json");
        } catch {
            failed = true;
            return;
        }

        List<WikiData> wikiDatas = new();

        foreach(var file in files) {
            using var reader = new StreamReader(file);
            string content = await reader.ReadToEndAsync();
            var json = JObject.Parse(content);

            string title = json["title"]?.ToString() ?? null;
            if(title == null) {
                continue;
            }

            WikiData wikiData = new() {
                Title = title
            };

            if(json["body"] is not JArray bodyArray) {
                continue;
            } else {
                foreach(var section in bodyArray) {
                    string sectionTitle = section["title"]?.ToString();
                    string sectionContent = section["content"]?.ToString();

                    if(sectionTitle != null && sectionContent != null) {
                        wikiData.Sections.Add(new Body {
                            Title = sectionTitle,
                            Content = WikiParser.Parse(sectionContent)
                        });
                    }
                }
            }

            string language = json["language"]?.ToString() ?? "default";
            wikiData.Language = language;

            wikiDatas.Add(wikiData);
        }

        failed = false;
        OnLoaded(wikiDatas);
        IsLoading = false;
    }
}
