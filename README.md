# Shortbook

## Background

**This is a Kindle book shortener using C#.** It "unlocks" the DRM on books you bought by automatically paging through the Windows Kindle app and taking screenshots of every page, then doing OCR on each and combining into a single file (there's smarter ways, but this means I don't need external DeDRM libraries, which also tend to break on Kindle updates).

*Please utilize for private use only, on books you already own, in jurisdictions where circumventing the DRM is legal.*

The generated text file is then separated into equal chunks and shortened via the **GPT-4 API**, while keeping style and narrative perspective and protagonist speech and such (but all shortened). The result can then additionally be translated to another language, like German. And then the whole shortened thing is sent back to Kindle for reading.

It's a bit like Blinkist, but intentionally a tad longer, and it works on any book, including novels.

Some books you want to read in full, cherishing every word and stylistic choice, but this shortener is great for the remaining portion of books you want to have read, but for which you realistically don't find the time. And you can then still switch to the longer version mid-through if it grips you.

One interesting thing is that once you have this chain, you can easily adjust the prompt to, say, rewrite the book for kids, or transpose the setting into another time, or replace one aspect of the story with another. For instance, you could put an Asimov robot story into the year 2025... and have the robot manufacturer be called OpenAI.

The program still requires some hand-holding and takes a bit of time to run, but I hope in the future some ebook vendor comes along and delivers it all directly. There could be a slider to determine the length of the book, and another slider to adjust the language difficulty. And maybe Blinkist or a competitor of theirs also starts utilizing AI.

## Installation

1. Buy a Kindle fiction or non-fiction book (in any language) which you want to summarize.
2. Install the [Kindle Windows app](https://www.amazon.com/kindlepcdownload/).
3. Tune so you get a full-screen and full-width page display with a readable small font, as shown in the screenshot, and page to page 1.

![image](https://github.com/JPhilipp/Shortbook/assets/1754503/b1a27fc2-1090-4e58-b538-17917e115b5e)

4. Open the project in e.g. VS Code, install the C# extension, and set all the appropriate setting values in the `Main()` function. This will need some care and trying out, e.g. to get the correct number of right-arrow-presses (the maxPages number) which the program will trigger.
5. Out-comment the different process functions one at a time, starting with `SavePagesAsImages()`, and always hit F5 to run.
6. When all is finished, send the resulting Summary.txt file to your Kindle library [via this page](https://www.amazon.com/sendtokindle) and wait a bit until it appears.
