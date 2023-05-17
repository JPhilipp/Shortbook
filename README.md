# Shortbook

I wrote a Kindle book shortener using C#. It "cracks" the DRM on books I bought by automatically paging through the Windows Kindle app and taking screenshots of every page, then doing OCR on each and combining into a single file (there's smarter ways, but this means I don't need external DeDRM libraries, which also tend to break on Kindle updates). That text file is then separated into equal chunks and shortened via the GPT-4 API, while keeping style and narrative perspective and protagonist speech and such (but all shortened). The result can then additionally be translated to another language, like German. And then the whole shortened thing is sent back to Kindle for reading.

It's a bit like Blinkist, but intentionally a tad longer, and it works on any book, including novels.

Some books you want to read in full, cherishing every word and stylistic choice, but this shortener is great for the remaining portion of books you want to have read, but for which you realistically don't find the time. And you can then still switch to the longer version mid-through if it grips you.

One interesting thing is that once you have this chain, you can easily adjust the prompt to, say, rewrite the book for kids, or transpose the setting into another time, or replace one aspect of the story with another. For instance, you could put an Asimov robot story into the year 2025... and have the robot manufacturer be called OpenAI.

The program still requires some handholding and takes a bit of time to run, but I hope in the future some ebook vendor comes along and delivers it all directly. There could be a slider to determine the length of the book, and another slider to adjust the language difficulty. And maybe Blinkist or a competitor of theirs also starts utilizing AI.