﻿namespace ExRam.Gremlinq.Core.Models
{
    public interface IGraphModel
    {
        IGraphModel ConfigureVertices(Func<IGraphElementModel, IGraphElementModel> transformation);
        IGraphModel ConfigureEdges(Func<IGraphElementModel, IGraphElementModel> transformation);
        IGraphModel ConfigureProperties(Func<IGraphElementPropertyModel, IGraphElementPropertyModel> transformation);

        IGraphElementModel VerticesModel { get; }
        IGraphElementModel EdgesModel { get; }
        IGraphElementPropertyModel PropertiesModel { get; }
    }
}
