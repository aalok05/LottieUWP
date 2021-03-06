﻿using System.Collections.Generic;

namespace LottieUWP
{
    internal class MergePathsContent : IPathContent
    {
        private readonly Path _firstPath = new Path();
        private readonly Path _remainderPath = new Path();
        private readonly Path _path = new Path();

        private readonly IList<IPathContent> _pathContents = new List<IPathContent>();
        private readonly MergePaths _mergePaths;

        internal MergePathsContent(MergePaths mergePaths)
        {
            Name = mergePaths.Name;
            _mergePaths = mergePaths;
        }

        internal virtual void AddContentIfNeeded(IContent content)
        {
            if (content is IPathContent pathContent)
            {
                _pathContents.Add(pathContent);
            }
        }

        public void SetContents(IList<IContent> contentsBefore, IList<IContent> contentsAfter)
        {
            for (var i = 0; i < _pathContents.Count; i++)
            {
                _pathContents[i].SetContents(contentsBefore, contentsAfter);
            }
        }

        public virtual Path Path
        {
            get
            {
                _path.Reset();

                switch (_mergePaths.Mode.InnerEnumValue)
                {
                    case MergePaths.MergePathsMode.InnerEnum.Merge:
                        AddPaths();
                        break;
                    case MergePaths.MergePathsMode.InnerEnum.Add:
                        OpFirstPathWithRest(Op.Union);
                        break;
                    case MergePaths.MergePathsMode.InnerEnum.Subtract:
                        OpFirstPathWithRest(Op.ReverseDifference);
                        break;
                    case MergePaths.MergePathsMode.InnerEnum.Intersect:
                        OpFirstPathWithRest(Op.Intersect);
                        break;
                    case MergePaths.MergePathsMode.InnerEnum.ExcludeIntersections:
                        OpFirstPathWithRest(Op.Xor);
                        break;
                }

                return _path;
            }
        }

        public string Name { get; }

        private void AddPaths()
        {
            for (var i = 0; i < _pathContents.Count; i++)
            {
                _path.AddPath(_pathContents[i].Path);
            }
        }

        private void OpFirstPathWithRest(Op op)
        {
            _remainderPath.Reset();
            _firstPath.Reset();

            for (var i = _pathContents.Count - 1; i >= 1; i--)
            {
                var content = _pathContents[i];

                if (content is ContentGroup contentGroup)
                {
                    var pathList = contentGroup.PathList;
                    for (var j = pathList.Count - 1; j >= 0; j--)
                    {
                        var path = pathList[j].Path;
                        path.Transform(contentGroup.TransformationMatrix);
                        _remainderPath.AddPath(path);
                    }
                }
                else
                {
                    _remainderPath.AddPath(content.Path);
                }
            }

            var lastContent = _pathContents[0];
            if (lastContent is ContentGroup group)
            {
                var pathList = group.PathList;
                for (var j = 0; j < pathList.Count; j++)
                {
                    var path = pathList[j].Path;
                    path.Transform(group.TransformationMatrix);
                    _firstPath.AddPath(path);
                }
            }
            else
            {
                _firstPath.Set(lastContent.Path);
            }

            _path.Op(_firstPath, _remainderPath, op);
        }
    }
}