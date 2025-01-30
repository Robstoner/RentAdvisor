import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faUser } from '@fortawesome/free-solid-svg-icons';
import axios from '../api/axiosConfig';
import '../css/PropertyDetails.css';
import '../css/PropertyCard.css';

import Carousel from "react-multi-carousel";
import "react-multi-carousel/lib/styles.css";

import { User, Property, Review } from "../App.tsx";

const PropertyDetails: React.FC = () => {
    const { propertyId } = useParams<{ propertyId: string }>();
    const [property, setProperty] = useState<Property | null>(null);
    const [reviews, setReviews] = useState<Review[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [newReview, setNewReview] = useState<{ title: string; description: string }>({
        title: '',
        description: ''
    });
    const [submittingReview, setSubmittingReview] = useState<boolean>(false);
    const [currentUser, setCurrentUser] = useState<User | null>(null);
    const [imageUrls, setImageUrls] = useState<string[]>([]);
    const navigate = useNavigate();

    // Fetch current user information
    useEffect(() => {
        const fetchCurrentUser = async () => {
            try {
                const response = await axios.get<User>('/User/current');
                setCurrentUser(response.data);
            } catch (error) {
                setError('Failed to fetch current user information.');
                console.error('Error fetching current user:', error);
            }
        };

        fetchCurrentUser();
    }, []);

    // Fetch property details and reviews
    useEffect(() => {
        const fetchPropertyDetails = async () => {
            try {
                const propertyResponse = await axios.get<Property>(`/api/Properties/${propertyId}`);
                setProperty(propertyResponse.data);

                // Fetch images for all photos
                if (propertyResponse.data.photos && propertyResponse.data.photos.length > 0) {
                    const imagePromises = propertyResponse.data.photos.map(async (photo) => {
                        try {
                            const response = await axios.get(`/api/Properties/photos/${photo.id}`, {
                                responseType: "blob",
                            });
                            return URL.createObjectURL(new Blob([response.data]));
                        } catch (error) {
                            console.error("Error fetching image:", error);
                            return null;
                        }
                    });

                    const images = await Promise.all(imagePromises);
                    setImageUrls(images.filter((img) => img !== null) as string[]);
                }

                // Fetch reviews
                const reviewsResponse = await axios.get<Review[]>(`/api/Properties/${propertyId}/Reviews`);
                setReviews(reviewsResponse.data);
            } catch (error) {
                setError('Failed to fetch property details or reviews.');
                console.error('Error fetching property details or reviews:', error);
            } finally {
                setLoading(false);
            }
        };

        if (propertyId) {
            fetchPropertyDetails();
        }
    }, [propertyId]);

    // Handle property deletion
    const handleDelete = async () => {
        const confirmDelete = window.confirm("Are you sure you want to delete this property? This action cannot be undone.");
        if (!confirmDelete) {
            return;
        }
        try {
            if (currentUser?.roles.includes('Admin')) {
                await axios.delete(`/api/Properties/${propertyId}`);
                navigate('/'); // Redirect to the home page after deletion
            } else {
                alert('You do not have permission to delete this property.');
            }
        } catch (error) {
            setError('Failed to delete property. Please try again.');
            console.error('Error deleting property:', error);
        }
    };
    // Navigate to the edit page
    const handleEdit = () => {
        if (currentUser?.roles.includes('Admin') || currentUser?.roles.includes('PropertyOwner') || currentUser?.roles.includes('Moderator')) {
            navigate(`/propertyEdit/${propertyId}`);
        } else {
            alert('You do not have permission to edit this property.');
        }
    };

    // Handle input change for new review form
    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        setNewReview((prevReview) => ({
            ...prevReview,
            [name]: value
        }));
    };

    // Handle submission of the new review
    const handleReviewSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        if (!newReview.title || !newReview.description) {
            setError('All fields are required to submit a review.');
            return;
        }

        setSubmittingReview(true);
        try {
            const reviewPayload = {
                ...newReview,
                propertyId,
                userId: currentUser?.id
            };

            await axios.post('/api/Reviews', reviewPayload);

            // Reset form
            setNewReview({ title: '', description: '' });

            // Refresh reviews after successful submission
            const updatedReviewsResponse = await axios.get<Review[]>(`/api/Properties/${propertyId}/Reviews`);
            setReviews(updatedReviewsResponse.data);
        } catch (error) {
            setError('Failed to submit the review. Please try again.');
            console.error('Error submitting review:', error);
        } finally {
            setSubmittingReview(false);
        }
    };

    // Handle review deletion
    const handleDeleteReview = async (reviewId: string, userId: string) => {
        try {
            if (currentUser?.roles.includes('Admin') || currentUser?.roles.includes('Moderator') || currentUser?.id === userId) {
                await axios.delete(`/api/reviews/${reviewId}`);
                // Refresh reviews after successful deletion
                const updatedReviewsResponse = await axios.get<Review[]>(`/api/Properties/${propertyId}/Reviews`);
                setReviews(updatedReviewsResponse.data);
            } else {
                alert('You do not have permission to delete this review.');
            }
        } catch (error) {
            setError('Failed to delete review. Please try again.');
            console.error('Error deleting review:', error);
        }
    };

    // Handle review edit
    const handleEditReview = (reviewId: string, userId: string) => {
        if (currentUser?.roles.includes('Admin') || currentUser?.roles.includes("Moderator") || currentUser?.id === userId) {
            // edit review
            reviewId = reviewId + 'test';
        } else {
            alert('You do not have permission to edit this review.');
        }
    };

    if (loading) {
        return <div>Loading property details...</div>;
    }

    if (error) {
        return <div className="error-message">{error}</div>;
    }

    if (!property) {
        return <div>Property not found</div>;
    }

    const responsive = {
        superLargeDesktop: { breakpoint: { max: 4000, min: 1024 }, items: 3 },
        desktop: { breakpoint: { max: 1024, min: 768 }, items: 2 },
        tablet: { breakpoint: { max: 768, min: 480 }, items: 1 },
        mobile: { breakpoint: { max: 480, min: 0 }, items: 1 }
    };

    return (
        <div className="main-container">

            <div className='top-content'>
                <div className='property-image-container'>
                {imageUrls.length > 0 ? (
                    <Carousel responsive={responsive} infinite autoPlay autoPlaySpeed={3000} showDots arrows>
                        {imageUrls.map((image, index) => (
                            <div key={index} className="carousel-slide">
                                <img src={image} alt={`Property Image ${index + 1}`} className="property-image" />
                            </div>
                        ))}
                    </Carousel>
                ) : (
                    <p>No images available</p>
                )}
                </div>
                <div className='property-title'>
                    <h1 className="property-name">{property.name}</h1>
                </div>
                <div className="propertybody">
                    <p className="property-address">{property.address}</p>
                    <p className="property-description">{property.description}</p>
                </div>
                <div className="property-features">
                    <ul>
                        {property.features.map((feature, index) => (
                            <li key={index}>{feature}</li>
                        ))}
                    </ul>
                </div>
                <div className="property-dates">
                    <p>Listed on: {new Date(property.createdAt).toLocaleDateString()}</p>
                    <p>Last updated: {new Date(property.updatedAt).toLocaleDateString()}</p>
                </div>
                <div className="button-container">
                    {currentUser?.roles.includes('Admin') || currentUser?.roles.includes('PropertyOwner') || currentUser?.roles.includes('Moderator') ? (
                        <button onClick={handleEdit} className="edit-button">Edit Property</button>
                    ) : null}
                    {currentUser?.roles.includes('Admin') || currentUser?.roles.includes('PropertyOwner') ? (
                        <button onClick={handleDelete} className="delete-button">Delete Property</button>
                    ) : null}
                </div>
            </div>

            <div className="new-review-form">
                <h3>Submit a Review</h3>
                <form onSubmit={handleReviewSubmit}>
                    <input
                        type="text"
                        name="title"
                        placeholder="Review Title"
                        value={newReview.title}
                        onChange={handleInputChange}
                        required
                    />
                    <textarea
                        name="description"
                        placeholder="Your Review"
                        value={newReview.description}
                        onChange={handleInputChange}
                        required
                    />
                    <div className='button-group'>
                        <button type="submit" disabled={submittingReview}>
                            {submittingReview ? 'Submitting...' : 'Submit Review'}
                        </button>
                    </div>
                </form>
            </div>

            <div className="property-reviews">
                <h3>Reviews:</h3>
                {reviews.length > 0 ? (
                    reviews.map((review) => (
                        <div key={review.id} className="review-card">
                            <p><FontAwesomeIcon icon={faUser} /> {review.userId}</p>
                            <strong>Posted on:</strong> {new Date(review.createdAt).toLocaleDateString()}
                            <h4>{review.title}</h4>
                            <p className="review-description">{review.description}</p>
                            <p className="review-meta">
                            </p>
                            {currentUser?.roles.includes('Admin') || currentUser?.roles.includes('Moderator') || currentUser?.id === review.userId ? (
                                <button onClick={() => handleEditReview(review.id, review.userId)} className="edit-review-button">Edit Review</button>
                            ) : null}
                            {currentUser?.roles.includes('Admin') || currentUser?.roles.includes('Moderator') || currentUser?.id === review.userId ? (
                                <button onClick={() => handleDeleteReview(review.id, review.userId)} className="delete-review-button">Delete Review</button>
                            ) : null}
                        </div>
                    ))
                ) : (
                    <p>No reviews available for this property.</p>
                )}
            </div>


        </div>
    );
};

export default PropertyDetails;
