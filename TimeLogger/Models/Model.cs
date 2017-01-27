using System.Collections.Generic;
using TimeLogger.DTO;
using TimeLogger.Services;

namespace TimeLogger.Models
{
    internal class Model
    {
        //--------------------------------------------------------------------------------
        //
        //      wiring
        //
        //--------------------------------------------------------------------------------

        private static readonly Model _instance = new Model();
        internal static Model instance { get { return _instance; } }
        static Model() { }
        Model()
        {
            init();
        }

        //--------------------------------------------------------------------------------
        //
        //      events
        //
        //--------------------------------------------------------------------------------

        internal delegate void tagsChangedEvent();
        internal event tagsChangedEvent tagsChanged;

        private void tagsTableChangedHandler()
        {
            _tags = DataService.instance.getTags();
            tagsChanged?.Invoke();
        }

        //--------------------------------------------------------------------------------
        //
        //      storage
        //
        //--------------------------------------------------------------------------------

        private List<Tag> _tags;

        //--------------------------------------------------------------------------------
        //
        //      initialisation
        //
        //--------------------------------------------------------------------------------

        private void init()
        {
            //hook up listeners
            DataService.instance.tagsTableChanged += tagsTableChangedHandler;
            //initialise data
            _tags = DataService.instance.getTags();
        }

        //--------------------------------------------------------------------------------
        //
        //      getters
        //
        //--------------------------------------------------------------------------------

        internal List<Tag> tags { get { return _tags; } }

        internal Tag getTagByID(int id)
        {
            return _tags.Find(x => x.id == id);
        }

        //--------------------------------------------------------------------------------
        //
        //      setters
        //
        //--------------------------------------------------------------------------------

        internal void addTag(Tag tag)
        {
            DataService.instance.addTag(tag);
        }

        internal void updateTag(Tag tag)
        {
            DataService.instance.updateTag(tag);
        }

        internal void deleteTag(int id)
        {
            DataService.instance.deleteTag(id);
        }

    }
}
