using System;
using System.Linq;
using System.Xml.Linq;

public static class Normalizer
{
    public static void FullCleanup(XContainer xe)
    {
        NormalizeImages(xe);
        NormalizeParagraphs(xe);
        NormalizeFrames(xe);  
        NormalizeScripts(xe);  
        NormalizeMeta(xe);  
        NormalizeLinks(xe);  
    }
    
    public static void NormalizeImages(XContainer xe)
    {		
        var images = xe.DescendantsByLocalName("img");
        foreach (var img in images)
        {
            //Add style
            var exWidthList = img.Attributes().Where(a => a.Name.LocalName.ToLower() == "style");
            if (exWidthList.Any())
                exWidthList.First().Value = "max-width: 100%; height: auto;";
            else
                img.Add(new XAttribute("style", "max-width: 100%; height: auto;")); 
            //Explicit width            
            var width = img.Attributes().FirstOrDefault(a => 
                a.Name.LocalName.ToLower() == "width" );
            var height = img.Attributes().FirstOrDefault(a => 
                a.Name.LocalName.ToLower() == "height" );
            if (width != null)
            {
                try
                {
                    if (int.Parse(width.Value) > 300);
                    throw new Exception("NeedResizeException");
                }
                catch
                {
                    width.Remove();
                    if (height != null) height.Remove();                    
                }
            }
            //Remove bad attributes            
            var badAttributes = img.Attributes().Where(a => 
                a.Name.LocalName.ToLower() == "align" );
            foreach (var attr in badAttributes)
            {
                attr.Remove();
            }
            var src = img.AttributeValue("src");
            var bigSrc = img.AttributeValue("data-src_big");
            if (bigSrc != null)
            {
                bigSrc = bigSrc.Split('|').FirstOrDefault();
                src = bigSrc;
            }
            if (src.StartsWith("//"))
                src = "http:" + src;
            img.Attribute("src").Value = src;
        }
    }
    
    public static void NormalizeMltbl(XContainer xe)
    {		
        //Head
        var mlheads = xe
            .DescendantsByLocalName("div").Where(d => d
            .AttributeValue("class") != null &&
                d.AttributeValue("class").Contains("mlhead"))
            .ToList();
        foreach (var mlhead in mlheads)
        {
            var icon = mlhead.DescendantByLocalName("img");
            if (icon.AttributeValue("style") != null) icon.Attribute("style").Remove();
            icon.Add(
                new XAttribute("width", "100"),
                new XAttribute("height", "100"));
            var game = mlhead.DescendantByLocalName("h2");
            var dev = mlhead.DescendantByLocalName("h3");
            game.Name = "h3";
            dev.Name = "h4";
            icon.Remove();
            game.Remove();
            dev.Remove();
            mlhead.AddFirst(
                new XElement("table", 
                    new XAttribute("width", "100%"),
                    new XElement("tr",
                        new XElement("td", icon),
                        new XElement("td", game, dev,
                            new XAttribute("align", "left"),
                            new XAttribute("width", "100%")))));
        }
        //Tables
        var mltbls = xe
            .DescendantsByLocalName("div").Where(d => d
            .AttributeValue("class") != null &&
                d.AttributeValue("class").Contains("mltbl"))
            .ToList();
        foreach (var mltbl in mltbls)
        {
            mltbl.Name = "table";
            mltbl.Add(new XAttribute("width", "100%"));
        }
        //Rows
        var mlrows = xe
            .DescendantsByLocalName("div").Where(d => d
            .AttributeValue("class") != null &&
                d.AttributeValue("class").Contains("mlrow"))
            .ToList();
        foreach (var mlrow in mlrows)
        {
            mlrow.Name = "tr";
        }
        //Cells
        var mlcells = xe
            .DescendantsByLocalName("div").Where(d => d
            .AttributeValue("class") != null &&
                d.AttributeValue("class").Contains("mlcell"))
            .ToList();
        foreach (var mlcell in mlcells)
        {
            mlcell.Name = "td";
            mlcell.Add(new XAttribute("align", "center"));
            if (mlcell.Value.Contains(Const.Ruble))
                mlcell.Value = mlcell.Value.Replace(Const.Ruble, 'p');
        }
        //Buttons
        var buttons = xe
            .DescendantsByLocalName("a").Where(d => d
            .AttributeValue("class") != null &&
                d.AttributeValue("class").Contains("sc-btn"))
            .ToList();
        foreach (var button in buttons)
        {
            button.Remove();
        }
        //Screenshots
        var screenshots = xe
            .DescendantsByLocalName("div").Where(d => d
            .AttributeValue("class") != null &&
                d.AttributeValue("class").Contains("mlscreenshots"))
            .ToList();
        foreach (var screenshot in screenshots)
        {
            screenshot.Attribute("class").Remove();
            var inner = screenshot.Element("div");
            inner.Remove();
            screenshot.Add(
                new XElement("br"),
                new XAttribute("class", "spoil"),
                new XElement("div",
                    new XAttribute("class", "smallfont"),
                    new XElement("input",
                        new XAttribute("class", "input-button"),
                        new XAttribute("type", "button"),
                        new XAttribute("value", "..."),
                        new XAttribute("onclick", Const.SpoilerScript))),
                new XElement("div",
                    new XAttribute("class", "alt2"),
                    new XElement("div",
                        new XAttribute("style", "display: none;"),
                        new XElement("br"),
                        inner)));            
        }
        //Icons
        var icons = xe
            .DescendantsByLocalName("i").Where(d => d
            .AttributeValue("class") != null &&
                d.AttributeValue("class").Contains("icon"))
            .ToList();
        foreach (var i in icons)
        {
            i.Name = "image";
            var rowClass = i.AttributeValue("class");
            switch (rowClass)
            {
                case "icon-apple":
                    i.Add(new XAttribute("src", Const.AppStoreIcon));
                    i.Add(new XAttribute("width", Const.IconSize));
                    i.Add(new XAttribute("height", Const.IconSize));
                    break;
                case "icon-android":
                    i.Add(new XAttribute("src", Const.GooglePlayIcon));
                    i.Add(new XAttribute("width", Const.IconSize));
                    i.Add(new XAttribute("height", Const.IconSize));
                    break;
                case "icon-windows":
                    i.Add(new XAttribute("src", Const.WindowsStoreIcon));
                    i.Add(new XAttribute("width", Const.IconSize));
                    i.Add(new XAttribute("height", Const.IconSize));
                    break;
            }
        }
    }
    
    public static void NormalizeParagraphs(XContainer xe)
    {		  
        var paragraphs = xe.DescendantsByLocalName("p");
        var spans = xe.DescendantsByLocalName("span");
        paragraphs.AddRange(spans);
        foreach (var p in paragraphs)
        {
            var exJustifyList = p.Attributes().Where(a => a.Name.LocalName.ToLower() == "style");
            if (exJustifyList.Any())
                exJustifyList.First().Value = "text-align: left;";
            else
                p.Add(new XAttribute("style", "text-align: left;"));                
        }   
    }
    
    public static void NormalizeFrames(XContainer xe)
    {		
        var frames = xe.DescendantsByLocalName("iframe");
        foreach (var frame in frames)
        {
            var src = frame.AttributeValue("src");
            if (src.Contains("youtube"))
            {
                src = src.Split('/').Last();
                src = src.Split('?').First();
                var metrotube = "{4}" + src;
                var preview = String.Format("http://img.youtube.com/vi/{0}/default.jpg", src);
                var divContainerStyle = "position: relative;";
                var divImageStyle = "position: absolute; top: 0; left: 0; width: 100%; height: 100%;";
                var divTextStyle = "position: absolute; top: 50; left: 0; width: 100%; height: 100%;";
                frame.Name = "a";
                frame.RemoveAttributes();
                frame.Add(new XAttribute("href", metrotube));
                frame.Add(
                    new XElement("center",
                        new XElement("div",
							new XAttribute("style", divContainerStyle),
							new XElement("div",
								new XAttribute("style", divImageStyle),
								new XElement("img",
									new XAttribute("src", preview),
									new XAttribute("width", 300),
									new XAttribute("height", 169))),
							new XElement("div",
								new XAttribute("style", divImageStyle),
								new XElement("img",
									new XAttribute("src", "http://cherrywebxslt.azurewebsites.net/App_Resources/ytb.png"),
									new XAttribute("width", 300),
									new XAttribute("height", 169)))),                                            
                        new XElement("table",
							new XAttribute("height", 169),
                            new XElement("tr",
                                new XElement("td")))));
            }
            else
            {
                frame.Remove();
            }
        }
    }
    
    public static void NormalizeScripts(XContainer xe)
    {		
        var scripts = xe.DescendantsByLocalName("script");
        var styles = xe.DescendantsByLocalName("style");
        scripts.AddRange(styles);
        foreach (var script in scripts)
        {
            script.Remove();
        }
    }
    
    public static void NormalizeLinks(XContainer xe)
    {		
        var links = xe.DescendantsByLocalName("a");
        foreach (var link in links)
        {
            var target = link.Attributes().Where(a => a.Name.LocalName.ToLower() == "target");
            if (target.Any())
                target.First().Value = "_blank";
            else
                link.Add(new XAttribute("target", "_blank")); 
        }
        var emptyLinks = links.Where(a => a.AttributeValue("href") == null);
        foreach (var link in emptyLinks)
        {
            link.Remove();
        }
    }
    
    public static void NormalizeMeta(XContainer xe)
    {		
        var metas = xe.DescendantsByLocalName("meta");
        foreach (var meta in metas)
        {
            meta.Remove();
        }
    }
}