import "../css/PropertyCard.css";

type Property = {
    id: string;
    name: string;
    address: string;
    description: string;
    features: string[];
    createdAt: string;
    updatedAt: string;
};

type PropertyCardProps = {
    property: Property;
};

const PropertyCard: React.FC<PropertyCardProps> = ({ property }) => {
    return (
        <div className="property-card">
            <div className="property-left">
            </div>
            <div className="property-right">
                <div className="property-title">
                    <h2 className="property-name">{property.name}</h2>
                </div>
                <div className="propertybody">
                    <p className="property-address">{property.address}</p>
                    <p className="property-description">{property.description}</p>
                </div>
                <div className="property-features">
                    <h3>Features:</h3>
                    <ul>
                        {property.features.map((feature, index) => (
                            <li key={index} className="property-feature">{feature}</li>
                        ))}
                    </ul>
                </div>
                <p className="property-dates">
                    Listed on: {new Date(property.createdAt).toLocaleDateString()} <br />
                    Last updated: {new Date(property.updatedAt).toLocaleDateString()}
                </p>
            </div>
        </div>
    );
};

export default PropertyCard;
