// See https://aka.ms/new-console-template for more information

using Dao.SingleSiteDataLock;

Console.WriteLine("Hello, World!");

var users = new List<string>();
for (var i = 0; i < 10; i++)
{
    users.Add(Guid.NewGuid().ToString());
}

for (var i = 0; i < 30; i++)
{
    var index = i;
    users.ParallelForEach(e =>
    {
        using (var scope = new ScopedLockContext())
        {
            switch (index % 2)
            {
                case 0: {
                    var get = scope.TryWriterLock("", e);
                    if (get)
                        Console.WriteLine($"TryWriterLock: {get}");
                    break;
                }
                case 1: {
                    var get = scope.TryReaderLock("", e);
                    Console.WriteLine($"TryReaderLock: {get}");
                    break;
                }
                //case 2:
                //    Console.WriteLine($"IsWriterLocked: {scope.IsWriterLocked("", e)}");
                //    break;
                //case 3:
                //    var view = scope.GetView();
                //    break;
            }

            Console.WriteLine($"IsWriterLocked: {scope.IsWriterLocked("", e)}");
            Task.Delay(index * 10).GetAwaiter().GetResult();

            switch (index % 3)
            {
                case 0:
                    var released = scope.ReleaseWriterLock("", e);
                    if (!released)
                        Console.WriteLine($"ReleaseWriterLock: {released}");

                    break;
                case 1:
                    scope.ReleaseReaderLock("", e);
                    //Console.WriteLine($"ReleaseReaderLock: {scope.ReleaseReaderLock("", e)}");
                    break;
                //case 2:
                //    scope.Revert();
                //    break;
                //case 3:
                //    scope.ReleaseAll(e);
                //    break;
            }
        }
    });
}


Console.ReadLine();