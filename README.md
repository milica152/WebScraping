# WebScraping

Ovo je aplikacija za scrape-ovanje podataka sa sajta https://srh.bankofchina.com/search/whpj/searchen.jsp.
Sajtu se pristupa putem klase HttpWebRequest, metodama GET i POST.
Inicijalni HTML sajt se dobija GET zahtevom, dok se HTML za scrape-ovanje vraćenih podataka dobija automatskim popunjavanjem forme i slanjem POST zahteva na sajt.
Pomoću biblioteke HtmlAgilityPack i XPath izraza parsiraju se dobijene HTML stranice i dobijaju vrednosti u njima.
Sa scrape-ovanjem se staje kada se broj stranice u HTML dokumentu više ne podudara sa brojem stranice koja je poslata u POST requestu.
Svi scrape-ovani podaci se zatim zapisuju u CSV fajl čija se putanja specificira u appSettings.json fajlu u okviru projekta.
